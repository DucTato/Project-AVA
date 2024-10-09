using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneHandler : MonoBehaviour
{
    private GamepadControls gpControls;
    private Rigidbody rb;
    [Header("Plane Stats")]
    [SerializeField]
    [Tooltip("How much the throttle ramps up or down")]
    private float throttleIncrement = 0.1f;
    [SerializeField]
    [Tooltip("Maximum engine thrust at 100% throttle")]
    private float maxThrust = 200f;
    [SerializeField]
    [Tooltip("How responsive the plane is when rolling, pitching, and yawing")]
    private float responsiveness = 10f;
    [SerializeField]
    private float liftForce = 135f;
    [SerializeField]
    [Tooltip("Adjust how much pitch the plane should get on top of the base responsiveness")]
    private float pitchModifier;
    private float throttle, throttleValueTrigger;
    private float roll, pitch, yaw;
    private Vector2 stickValue;

    // Taking the plane's mass into tweaking its responsiveness
    private float responsibilityModifier
    {
        get { return (rb.mass / 10f) * responsiveness; }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        gpControls = new GamepadControls();
        gpControls.Gameplay.ACmovement.performed += context => stickValue = context.ReadValue<Vector2>();
        gpControls.Gameplay.ACmovement.canceled += context => stickValue = Vector2.zero;

        gpControls.Gameplay.ACyaw.performed += context => yaw = context.ReadValue<float>();
        gpControls.Gameplay.ACyaw.canceled += context => yaw = 0f;

        gpControls.Gameplay.ACthrottle.performed += context => throttleValueTrigger = context.ReadValue<float>();
        gpControls.Gameplay.ACthrottle.canceled += context => throttleValueTrigger = 0f;

    }

    private void OnEnable()
    {
        gpControls.Gameplay.Enable();
    }
    private void OnDisable()
    {
        gpControls.Gameplay.Disable();
    }
    // Start is called before the first frame update
    void Start()
    {
        yaw = 0f;
        throttle = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
      
    }
    private void FixedUpdate()
    {
        // Update physics in this callback to avoid jittering!

        // Applying forces to the plane
        rb.AddForce(maxThrust * throttle * transform.forward);
        // Torque is rotational force
        rb.AddTorque(yaw * responsibilityModifier * transform.up);
        rb.AddTorque(pitch * pitchModifier * responsibilityModifier * transform.right);
        rb.AddTorque(-roll * responsibilityModifier * transform.forward);
        // Adding lift when the plane is horizontal relative to the ground
        rb.AddForce(rb.velocity.magnitude * liftForce * transform.up);
    }
    //private void HandleInput()    // Legacy input system
    //{
    //    // Setting rotational values from axis inputs
    //    roll = Input.GetAxis("Roll");
    //    pitch = Input.GetAxis("Pitch");
    //    yaw = Input.GetAxis("Yaw");
    //    //Debug.Log(yaw);
    //    // Handling throttle inputs
    //    if (Input.GetKey(KeyCode.W))
    //    {
    //        throttle += throttleIncrement;
    //    }
    //    else if (Input.GetKey(KeyCode.S))
    //    {
    //        throttle -= throttleIncrement;
    //    }
    //    throttle = Mathf.Clamp(throttle, 0f, 100f);
    //    Debug.Log("Current throttle: " + throttle);
    //}

    private void HandleInput()
    {
        roll = stickValue.x;
        pitch = stickValue.y;    // pitching is always more effective 
        throttle += (throttleIncrement * throttleValueTrigger);
        throttle = Mathf.Clamp(throttle, 0f, 100f);
    }
}
