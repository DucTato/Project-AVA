using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
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
    private float throttle;
    private float roll, pitch, yaw;
    
    // Taking the plane's mass into tweaking its responsiveness
    private float responsibilityModifier
    {
        get { return (rb.mass / 10f) * responsiveness; }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
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
        rb.AddTorque(pitch * responsibilityModifier * transform.right);
        rb.AddTorque(-roll * responsibilityModifier * transform.forward);
        // Adding lift when the plane is horizontal relative to the ground
        rb.AddForce(rb.velocity.magnitude * liftForce * transform.up);
    }
    private void HandleInput()
    {
        // Setting rotational values from axis inputs
        roll = Input.GetAxis("Roll");
        pitch = Input.GetAxis("Pitch");
        yaw = Input.GetAxis("Yaw");

        // Handling throttle inputs
        if(Input.GetKey(KeyCode.W))
        {
            throttle += throttleIncrement;
        }
        else if(Input.GetKey(KeyCode.S))
        {
            throttle -= throttleIncrement;
        }
        throttle = Mathf.Clamp(throttle, 0f, 100f);
    }
}
