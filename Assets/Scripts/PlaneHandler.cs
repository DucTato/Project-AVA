using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneHandler : MonoBehaviour
{
    
    private Rigidbody rb;
    [Header("Plane Stats")]
    private Vector3 lastVelocity, controlInput, localGForce;
    private float throttleInput;
    private bool isAlive;
    [SerializeField]
    private float gLimit, gLimitPitch;
    [SerializeField]
    private Vector3 localVelocity, velocity, localAngularVelocity;
    [SerializeField]
    private Vector3 effectiveInput;

    [Header("Thrust")]
    [SerializeField]
    [Tooltip("Maximum engine thrust at 100% throttle")]
    private float maxThrust = 200f;
    [SerializeField]
    private float throttle, throttleSpeed;

    [Header("Drag")]
    [SerializeField]
    private float airBrakeDrag, flapsDrag, inducedDrag;
    [SerializeField]
    private AnimationCurve dragRight, dragLeft, dragTop, dragBottom, dragForward, dragBack;
    [SerializeField]
    private Vector3 angularDrag;

    [SerializeField]
    private float angleOfAttack, angleOfAttackYaw;

    [Header("Lift")]
    [SerializeField]
    private float flapsLiftPower, liftPower, flapsAoABias, flapsRetractSpeed;
    [SerializeField]
    private AnimationCurve liftAOACurve, inducedDragCurve, rudderAOACurve, rudderInducedDragCurve;

    [SerializeField]
    private bool AirBrakeDeployed, FlapsDeployed;
    
    [SerializeField]
    private float rudderPower;

    [Header("Steering")]
    [SerializeField]
    private AnimationCurve steeringCurve;
    private Vector3 turnSpeed;
    [SerializeField]
    private Vector3 turnAcceleration;



    #region Basic CallBacks
    private void Awake()
    {
        isAlive = true;
        rb = GetComponent<Rigidbody>();
        

        //gpControls.Gameplay.ACyaw.performed += context => yaw = context.ReadValue<float>();
        //gpControls.Gameplay.ACyaw.canceled += context => yaw = 0f;

        //gpControls.Gameplay.ACthrottle.performed += context => throttleValueTrigger = context.ReadValue<float>();
        //gpControls.Gameplay.ACthrottle.canceled += context => throttleValueTrigger = 0f;

    }

    
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        // Update physics in this callback to avoid jittering!

        float dt = Time.fixedDeltaTime;
        // Calculate at start to capture any changes that happened externally
        CalculateState(dt);
        CalculateGForce(dt);
        UpdateFlaps();
        // Handle player input
        UpdateThrottle(dt);

        // Update plane's states
        UpdateThrustValue();
        UpdateLift();

        if (isAlive)    UpdateSteering(dt);

        UpdateDrag();
        UpdateAngularDrag();
        // Calculate again

        CalculateState(dt);
    }
    #endregion
    #region Plane Handling
    private void UpdateFlaps()
    {
        if (localVelocity.z > flapsRetractSpeed) FlapsDeployed = false;
    }
    private void CalculateState(float dt, bool firstThisFrame = true)
    {
        var inverseRotation = Quaternion.Inverse(rb.rotation);
        velocity = rb.velocity;
        localVelocity = inverseRotation * velocity;
        localAngularVelocity = inverseRotation * rb.angularVelocity;
        CalculateAngleOfAttack();
    }
    private void CalculateAngleOfAttack()
    {
        angleOfAttack = Mathf.Atan2(-localVelocity.y, localVelocity.z);     // pitch axis
        angleOfAttackYaw = Mathf.Atan2(localVelocity.x, localVelocity.z);   // yaw axis
    }
    private void CalculateGForce(float dt)
    {
        var inverseRotation = Quaternion.Inverse(rb.rotation);
        var accel = (velocity - lastVelocity) / dt;
        localGForce = inverseRotation * accel;
        lastVelocity = velocity;
    }
    private Vector3 CalculateGForce(Vector3 angularVelocity, Vector3 velocity)
    {
        // estimate FUTURE G-FORCE for the G-Force limiter feature
        /*Velocity = AngularVelocity * Radius
         *G = Velocity^2 / Radius
         *G = (Velocity * AngularVelocity * Radius) / Radius
         *G = Velocity x AngularVelocity
         */
        return Vector3.Cross(angularVelocity, velocity);
    }
    private Vector3 CalculateGForceLimit(Vector3 input)
    {
        return Utilities.Scale6(input, gLimit, gLimitPitch, gLimit, gLimit, gLimit, gLimit);
    }
    private float CalculateGLimiter(Vector3 controlInput, Vector3 maxAngularVelocity, bool GLimiterON = false)
    {
        // The limiter is inactive when it returns 1f (100% of the pilot's input)
        if (GLimiterON) return 1f;
        /*maxInput is the input from the pilot but got 100%-ed
         * so that the maximum turn rate is in the direction
         * of the pilot
         */
        var maxInput = controlInput.normalized;
        var limit = CalculateGForceLimit(maxInput);
        var maxGForce = CalculateGForce(Vector3.Scale(maxInput, maxAngularVelocity), localVelocity);
        if (maxGForce.magnitude > limit.magnitude)
        {
            /*Example:
             * maxGForce = 16G, limit = 8G
             * return 8/16 = 0.5 (50% of the input from the pilot)
             */
            return limit.magnitude / maxGForce.magnitude;
        }
        return 1f;
    }
    private void UpdateThrustValue()
    {
        rb.AddRelativeForce(throttle * maxThrust * Vector3.forward);    // throttle can be 0 to let players fly without engine power (gliding)

    }
    private void UpdateDrag()
    {
        var lv = localVelocity;
        var lv2 = lv.sqrMagnitude;
        float airBrakeDrag = AirBrakeDeployed ? this.airBrakeDrag : 0f;
        float flapsDrag = FlapsDeployed ? this.flapsDrag : 0f;
        // Calculate the co-efficient of drag depending on the direction 
        var coefficient = Utilities.Scale6(lv.normalized, dragRight.Evaluate(Mathf.Abs(lv.x)), dragLeft.Evaluate(Mathf.Abs(lv.x)), 
                                            dragTop.Evaluate(Mathf.Abs(lv.y)), dragBottom.Evaluate(Mathf.Abs(lv.y)), 
                                            dragForward.Evaluate(Mathf.Abs(lv.z)) + airBrakeDrag + flapsDrag, // extra drag from forward coefficient
                                            dragBack.Evaluate(Mathf.Abs(lv.z)));
        var drag = coefficient.magnitude * lv2 * -lv.normalized;    // drag is the opposite direction of velocity
        rb.AddRelativeForce(drag);
    }
    private void UpdateAngularDrag()
    {
        var av = localAngularVelocity;
        var drag = av.sqrMagnitude * -av.normalized;    // drag is the opposite direction of angular velocity
        rb.AddRelativeForce(Vector3.Scale(drag, angularDrag), ForceMode.Acceleration);
    }
    private Vector3 CalculateLift(float AoA, Vector3 rightAxis, float liftPower, AnimationCurve aoaCurve, AnimationCurve inducedDragCurve)
    {
        var liftVelocity = Vector3.ProjectOnPlane(localVelocity, rightAxis);
        var v2 = liftVelocity.sqrMagnitude;    // liftVelocity squared

        var liftCoefficient = aoaCurve.Evaluate(AoA * Mathf.Rad2Deg);   // coefficient varies with AoA
        var liftForce = v2 * liftCoefficient * liftPower;
        var liftDirection = Vector3.Cross(liftVelocity.normalized, rightAxis);
        var lift = liftDirection * liftForce;

        var dragForce = liftCoefficient * liftCoefficient;
        var dragDirection = - liftVelocity.normalized;
        var inducedDrag = dragDirection * v2 * dragForce * this.inducedDrag * inducedDragCurve.Evaluate(Mathf.Max(0, localVelocity.z));
        return lift + inducedDrag;
    }
    private void UpdateLift()
    {
        float flapsLiftPower = FlapsDeployed ? this.flapsLiftPower : 0f;
        float flapsAOABias = FlapsDeployed ? this.flapsAoABias : 0f;
        var liftForce = CalculateLift(angleOfAttack + (flapsAOABias * Mathf.Deg2Rad), Vector3.right,
                                        liftPower + flapsLiftPower, liftAOACurve, inducedDragCurve);
        var yawForce = CalculateLift(angleOfAttackYaw, Vector3.up, rudderPower, rudderAOACurve, rudderInducedDragCurve);
        rb.AddRelativeForce(yawForce);
        rb.AddRelativeForce(liftForce);
    }
    private float CalculateSteering(float dt, float angularVelocity, float targetVelocity, float acceleration)
    {
        var error = targetVelocity - angularVelocity;
        var accel = acceleration * dt;
        return Mathf.Clamp(error, -accel, accel);
    }
    private void UpdateSteering(float dt)
    {
        var speed = Mathf.Max(0, localVelocity.z);
        var steeringPower = steeringCurve.Evaluate(speed);

        var gForceScaling = CalculateGLimiter(controlInput, turnSpeed * Mathf.Rad2Deg * steeringPower);
        var targetAV = Vector3.Scale(controlInput, steeringPower * turnSpeed * gForceScaling);

        var av = localAngularVelocity * Mathf.Rad2Deg;

        var correction = new Vector3(CalculateSteering(dt, av.x, turnAcceleration.x, steeringPower),
                                        CalculateSteering(dt, av.y, turnAcceleration.y, steeringPower),
                                        CalculateSteering(dt, av.z, turnAcceleration.z, steeringPower));
        rb.AddRelativeForce(correction * Mathf.Rad2Deg, ForceMode.VelocityChange);  // ignore rigidbody mass

        var correctionInput = new Vector3(Mathf.Clamp((targetAV.x - av.x) / turnAcceleration.x, -1, 1),
                                            Mathf.Clamp((targetAV.y - av.y) / turnAcceleration.y, -1, 1),
                                            Mathf.Clamp((targetAV.z - av.z) / turnAcceleration.z, -1, 1));
        var eInput = (correctionInput + controlInput) * gForceScaling;
        effectiveInput = new Vector3(Mathf.Clamp(eInput.x, -1, 1), Mathf.Clamp(eInput.y, -1, 1),Mathf.Clamp(eInput.z, -1, 1));                               
    }
    #endregion
    #region Input Handling
    public void SetThrottleInput (float inputThrottle)
    {
        if (isAlive) throttleInput = inputThrottle;
    }
    public void SetControlInput (Vector3 input)
    {
        if (isAlive) controlInput = input;
    }
    public void ToggleFlaps()
    {
        if (localVelocity.z < flapsRetractSpeed) FlapsDeployed = !FlapsDeployed;
    }
    private void UpdateThrottle(float dt)
    {
        /*Throttle input is [-1, 1]
         * ThrottleValue is [0, 1]
         */
        float target = 0;
        if (throttleInput > 0f) target = 1f;
        throttle = Utilities.MoveTo(throttle, target, throttleSpeed * Mathf.Abs(throttleInput), dt);
        AirBrakeDeployed = throttle == 0f && throttle == -1f;
    }
    #endregion
}
