using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeliHandler : MonoBehaviour
{
    private GamepadControls gpControls;
    private Rigidbody rb;
    [Header("Plane Stats")]
    [SerializeField]
    private float responsiveness;
    [SerializeField]
    private float throttleIncrement;
    [SerializeField]
    private float rotorSpeedMult;
    [SerializeField]
    [Tooltip("Maximum engine thrust at 100% throttle")]
    private float maxThrust;
    [SerializeField]
    [Tooltip("Strength of the auto leveling system, determines maximum AOA")]
    private float levelingStrength;
    [SerializeField]
    private Transform rotorTransform;
    private float throttle, throttleValueTrigger;
    private float roll, pitch, yaw;
    private Vector2 stickValue;
    private Vector3 levelValue;
    private InputAction throttleTriggerState;

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

        gpControls.Gameplay.ACthrottle.performed += context => {
            throttleValueTrigger = context.ReadValue<float>();
        };
        gpControls.Gameplay.ACthrottle.canceled += context => { 
            throttleValueTrigger = 0f;
        };
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
        throttleTriggerState = gpControls.Gameplay.ACthrottle;
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        rotorTransform.Rotate(maxThrust * throttle * rotorSpeedMult * Time.deltaTime * Vector3.up);
    }
    private void FixedUpdate()
    {
        // Update physics in this callback to avoid jittering!
        // Applying forces to the plane
        rb.AddForce(maxThrust * throttle * transform.up);
        rb.AddTorque(pitch * responsibilityModifier * transform.right);
        rb.AddTorque(-roll * responsibilityModifier * transform.forward);
        rb.AddTorque(yaw * responsibilityModifier * transform.up);
        AutoLeveling();
    }
    //private void HandleInput()      // Legacy input system
    //{
    //    // Setting rotational values from axis inputs
    //    roll = Input.GetAxis("Roll");
    //    pitch = Input.GetAxis("Pitch");
    //    yaw = Input.GetAxis("Yaw");

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
    //}

    private void HandleInput()
    {
        roll = stickValue.x;
        pitch = stickValue.y;
        throttle += (throttleIncrement * throttleValueTrigger);
        throttle = Mathf.Clamp(throttle, -30f, 100f);
        if (!throttleTriggerState.IsPressed())
        {
            // Throttle decay mechanic - throttle will gradually move toward a specified amount when idle
            throttle = Mathf.MoveTowards(throttle, 0f, 0.08f);
        }
    }
    private void AutoLeveling()
    {
        levelValue.x = transform.rotation.x;    // Levels the pitch value
        levelValue.z = transform.rotation.z;    // Levels the roll 
        rb.AddTorque(maxThrust * levelingStrength * -levelValue);
    }
}
