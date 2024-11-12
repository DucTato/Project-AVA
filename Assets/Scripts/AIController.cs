using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Windows;

public class AIController : MonoBehaviour
{
    [SerializeField]
    private PlaneHandler PlaneHandler;
    [SerializeField]
    private float steeringSpeed;
    [SerializeField]
    private float minSpeed;
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float recoverSpeedMin;
    [SerializeField]
    private float recoverSpeedMax;
    [SerializeField]
    private LayerMask groundCollisionMask;
    [SerializeField]
    private float groundCollisionDistance;
    [SerializeField]
    private float groundAvoidanceAngle;
    [SerializeField]
    private float groundAvoidanceMinSpeed;
    [SerializeField]
    private float groundAvoidanceMaxSpeed;
    [SerializeField]
    private float pitchUpThreshold;
    [SerializeField]
    private float fineSteeringAngle;
    [SerializeField]
    private float rollFactor;
    [SerializeField]
    private float yawFactor;
    [SerializeField]
    private bool canUseMissiles;
    [SerializeField]
    private bool canUseCannon;
    [SerializeField]
    private float missileLockFiringDelay;
    [SerializeField]
    private float missileFiringCooldown;
    [SerializeField]
    private float missileMinRange;
    [SerializeField]
    private float missileMaxRange;
    [SerializeField]
    private float missileMaxFireAngle;
    [SerializeField]
    private float bulletSpeed;
    [SerializeField]
    private float cannonRange;
    [SerializeField]
    private float cannonMaxFireAngle;
    [SerializeField]
    private float cannonBurstLength;
    [SerializeField]
    private float cannonBurstCooldown;
    [SerializeField]
    private float minMissileDodgeDistance;
    [SerializeField]
    private float reactionDelayMin;
    [SerializeField]
    private float reactionDelayMax;
    [SerializeField]
    private float reactionDelayDistance;

    private Target selfTarget;
    [SerializeField]
    private Target _currentTarget;
    private FCS fireControl;
    private Vector3 lastInput;
    private bool isRecoveringSpeed;

    private float missileDelayTimer;
    private float missileCooldownTimer;

    private bool cannonFiring;
    private float cannonBurstTimer;
    private float cannonCooldownTimer;

    struct ControlInput
    {
        public float time;
        public Vector3 targetPosition;
    }

    private Queue<ControlInput> inputQueue;

    private bool dodging;
    private Vector3 lastDodgePoint;
    private List<Vector3> dodgeOffsets;
    private const float dodgeUpdateInterval = 0.25f;
    private float dodgeTimer;

    private void OnEnable()
    {
        fireControl = GetComponent<FCS>();
        if (gameObject.CompareTag("Enemy")) 
        { 
            fireControl.targetingBehaviour = FCS.TgtBehaviourType.Enemies;
            fireControl.lockRange = Mathf.Infinity;
        }
        else fireControl.targetingBehaviour = FCS.TgtBehaviourType.Player_Allies;
    }
    private void OnDisable()
    {
        if (gameObject.CompareTag("Player")) fireControl.targetingBehaviour = FCS.TgtBehaviourType.Player;
    }
    void Start()
    {
        selfTarget = GetComponent<Target>();
        
        dodgeOffsets = new List<Vector3>();
        inputQueue = new Queue<ControlInput>();
    }
    private void TryFindTarget()
    {
        if (fireControl.currTarget != null)
        {
            _currentTarget = fireControl.currTarget;
        }
    }
    private Vector3 AvoidGround()
    {
        //roll level and pull up
        var roll = PlaneHandler.rb.rotation.eulerAngles.z;
        if (roll > 180f) roll -= 360f;
        return new Vector3(-1, 0, Mathf.Clamp(-roll * rollFactor, -1, 1));
    }

    private Vector3 RecoverSpeed()
    {
        //roll and pitch level
        var roll = PlaneHandler.rb.rotation.eulerAngles.z;
        var pitch = PlaneHandler.rb.rotation.eulerAngles.x;
        if (roll > 180f) roll -= 360f;
        if (pitch > 180f) pitch -= 360f;
        return new Vector3(Mathf.Clamp(-pitch, -1, 1), 0, Mathf.Clamp(-roll * rollFactor, -1, 1));
    }

    private Vector3 GetTargetPosition()
    {
        if (fireControl.currTarget == null)
        {
            return PlaneHandler.rb.position;
        }

        var targetPosition = fireControl.currTarget.Position;

        if (Vector3.Distance(targetPosition, PlaneHandler.rb.position) < cannonRange)
        {
            return Utilities.FirstOrderIntercept(PlaneHandler.rb.position, PlaneHandler.rb.velocity, bulletSpeed, targetPosition, fireControl.currTarget.Velocity);
        }

        return targetPosition;
    }

    private Vector3 CalculateSteering(float dt, Vector3 targetPosition)
    {
        if (fireControl.currTarget != null && _currentTarget.Health == 0)
        {
            return new Vector3();
        }

        var error = targetPosition - PlaneHandler.rb.position;
        error = Quaternion.Inverse(PlaneHandler.rb.rotation) * error;   //transform into local space

        var errorDir = error.normalized;
        var pitchError = new Vector3(0, error.y, error.z).normalized;
        var rollError = new Vector3(error.x, error.y, 0).normalized;
        var yawError = new Vector3(error.x, 0, error.z).normalized;

        var targetInput = new Vector3();

        var pitch = Vector3.SignedAngle(Vector3.forward, pitchError, Vector3.right);
        if (-pitch < pitchUpThreshold) pitch += 360f;
        targetInput.x = pitch;

        if (Vector3.Angle(Vector3.forward, errorDir) < fineSteeringAngle)
        {
            var yaw = Vector3.SignedAngle(Vector3.forward, yawError, Vector3.up);
            targetInput.y = yaw * yawFactor;
        }
        else
        {
            var roll = Vector3.SignedAngle(Vector3.up, rollError, Vector3.forward);
            targetInput.z = roll * rollFactor;
        }

        targetInput.x = Mathf.Clamp(targetInput.x, -1, 1);
        targetInput.y = Mathf.Clamp(targetInput.y, -1, 1);
        targetInput.z = Mathf.Clamp(targetInput.z, -1, 1);

        var input = Vector3.MoveTowards(lastInput, targetInput, steeringSpeed * dt);
        lastInput = input;

        return input;
    }

    private Vector3 GetMissileDodgePosition(float dt, Missile missile)
    {
        dodgeTimer = Mathf.Max(0, dodgeTimer - dt);
        var missilePos = missile.rb.position;

        var dist = Mathf.Max(minMissileDodgeDistance, Vector3.Distance(missilePos, PlaneHandler.rb.position));

        //calculate dodge points
        if (dodgeTimer == 0)
        {
            var missileForward = missile.rb.rotation * Vector3.forward;
            dodgeOffsets.Clear();

            //4 dodge points: up, down, left, right

            dodgeOffsets.Add(new Vector3(0, dist, 0));
            dodgeOffsets.Add(new Vector3(0, -dist, 0));
            dodgeOffsets.Add(Vector3.Cross(missileForward, Vector3.up) * dist);
            dodgeOffsets.Add(Vector3.Cross(missileForward, Vector3.up) * -dist);

            dodgeTimer = dodgeUpdateInterval;
        }

        //select nearest dodge point
        float min = float.PositiveInfinity;
        Vector3 minDodge = missilePos + dodgeOffsets[0];

        foreach (var offset in dodgeOffsets)
        {
            var dodgePosition = missilePos + offset;
            var offsetDist = Vector3.Distance(dodgePosition, lastDodgePoint);

            if (offsetDist < min)
            {
                minDodge = dodgePosition;
                min = offsetDist;
            }
        }

        lastDodgePoint = minDodge;
        return minDodge;
    }

    private float CalculateThrottle(float minSpeed, float maxSpeed)
    {
        float input = 0;

        if (PlaneHandler.LocalVelocity.z < minSpeed)
        {
            input = 1;
        }
        else if (PlaneHandler.LocalVelocity.z > maxSpeed)
        {
            input = -1;
        }

        return input;
    }

    private void CalculateWeapons(float dt)
    {
        if (fireControl.currTarget == null) return;

        if (canUseMissiles)
        {
            CalculateMissiles(dt);
        }

        if (canUseCannon)
        {
            CalculateCannon(dt);
        }
    }

    private void CalculateMissiles(float dt)
    {
        missileDelayTimer = Mathf.Max(0, missileDelayTimer - dt);
        missileCooldownTimer = Mathf.Max(0, missileCooldownTimer - dt);

        var error = fireControl.currTarget.Position - PlaneHandler.rb.position;
        var range = error.magnitude;
        var targetDir = error.normalized;
        var targetAngle = Vector3.Angle(targetDir, PlaneHandler.rb.rotation * Vector3.forward);

        if (!fireControl.MissileLocked || !(targetAngle < missileMaxFireAngle || (180f - targetAngle) < missileMaxFireAngle))
        {
            //don't fire if not locked or target is too off angle
            //can fire if angle is close to 0 (chasing) or 180 (head on)
            missileDelayTimer = missileLockFiringDelay;
        }

        if (range < missileMaxRange && range > missileMinRange && missileDelayTimer == 0 && missileCooldownTimer == 0)
        {
            fireControl.TryFireMissile();
            missileCooldownTimer = missileFiringCooldown;
        }
    }

    private void CalculateCannon(float dt)
    {
        if (_currentTarget.Health == 0)
        {
            cannonFiring = false;
            return;
        }

        if (cannonFiring)
        {
            cannonBurstTimer = Mathf.Max(0, cannonBurstTimer - dt);

            if (cannonBurstTimer == 0)
            {
                cannonFiring = false;
                cannonCooldownTimer = cannonBurstCooldown;
                fireControl.FireCannon(false);
            }
        }
        else
        {
            cannonCooldownTimer = Mathf.Max(0, cannonCooldownTimer - dt);

            var targetPosition = Utilities.FirstOrderIntercept(PlaneHandler.rb.position, PlaneHandler.rb.velocity, bulletSpeed, fireControl.currTarget.Position, fireControl.currTarget.Velocity);

            var error = targetPosition - PlaneHandler.rb.position;
            var range = error.magnitude;
            var targetDir = error.normalized;
            var targetAngle = Vector3.Angle(targetDir, PlaneHandler.rb.rotation * Vector3.forward);

            if (range < cannonRange && targetAngle < cannonMaxFireAngle && cannonCooldownTimer == 0)
            {
                cannonFiring = true;
                cannonBurstTimer = cannonBurstLength;
                fireControl.FireCannon(true);
            }
        }
    }

    private void SteerToTarget(float dt, Vector3 planePosition)
    {
        bool foundTarget = false;
        Vector3 steering = Vector3.zero;
        Vector3 targetPosition = Vector3.zero;

        var delay = reactionDelayMax;

        if (Vector3.Distance(planePosition, PlaneHandler.rb.position) < reactionDelayDistance)
        {
            delay = reactionDelayMin;
        }

        while (inputQueue.Count > 0)
        {
            var input = inputQueue.Peek();

            if (input.time + delay <= Time.time)
            {
                targetPosition = input.targetPosition;
                inputQueue.Dequeue();
                foundTarget = true;
            }
            else
            {
                break;
            }
        }

        if (foundTarget)
        {
            steering = CalculateSteering(dt, targetPosition);
        }

        PlaneHandler.SetControlInput(steering);
    }

    private void FixedUpdate()
    {
        TryFindTarget();
        if (_currentTarget == null || selfTarget.Dead) 
        { 
            
            return;
        }
        var dt = Time.fixedDeltaTime;

        Vector3 steering = Vector3.zero;
        float throttle;
        bool emergency = false;
        Vector3 targetPosition = _currentTarget.Position;

        var velocityRot = Quaternion.LookRotation(PlaneHandler.rb.velocity.normalized);
        var ray = new Ray(PlaneHandler.rb.position, velocityRot * Quaternion.Euler(groundAvoidanceAngle, 0, 0) * Vector3.forward);

        if (Physics.Raycast(ray, groundCollisionDistance + PlaneHandler.LocalVelocity.z, groundCollisionMask.value))
        {
            steering = AvoidGround();
            throttle = CalculateThrottle(groundAvoidanceMinSpeed, groundAvoidanceMaxSpeed);
            emergency = true;
        }
        else
        {
            var incomingMissile = selfTarget.GetIncomingMissile();
            if (incomingMissile != null)
            {
                if (dodging == false)
                {
                    //start dodging
                    dodging = true;
                    lastDodgePoint = PlaneHandler.rb.position;
                    dodgeTimer = 0;
                }

                var dodgePosition = GetMissileDodgePosition(dt, incomingMissile);
                steering = CalculateSteering(dt, dodgePosition);
                emergency = true;
            }
            else
            {
                dodging = false;
                targetPosition = GetTargetPosition();
            }

            if (incomingMissile == null && (PlaneHandler.LocalVelocity.z < recoverSpeedMin || isRecoveringSpeed))
            {
                isRecoveringSpeed = PlaneHandler.LocalVelocity.z < recoverSpeedMax;

                steering = RecoverSpeed();
                throttle = 1;
                emergency = true;
            }
            else
            {
                throttle = CalculateThrottle(minSpeed, maxSpeed);
            }
        }

        inputQueue.Enqueue(new ControlInput
        {
            time = Time.time,
            targetPosition = targetPosition,
        });

        PlaneHandler.SetThrottleInput(throttle);

        if (emergency)
        {
            if (isRecoveringSpeed)
            {
                //reduce steering strength while recovering speed
                steering.x = Mathf.Clamp(steering.x, -0.5f, 0.5f);
            }

            PlaneHandler.SetControlInput(steering);
        }
        else
        {
            SteerToTarget(dt, _currentTarget.Position);
        }

        CalculateWeapons(dt);
    }
}