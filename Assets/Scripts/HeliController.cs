using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeliController : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField]
    private float responsiveness = 500f;
    [SerializeField]
    private float throttleIncrement = 0.1f;
    [SerializeField]
    private float rotorSpeedMult = 10f;
    [SerializeField]
    private float maxThrust = 5f;
    [SerializeField]
    private Transform rotorTransform;
    private float throttle;
    private float roll, pitch, yaw;

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
        rotorTransform.Rotate(maxThrust * throttle * rotorSpeedMult * Vector3.up);
    }
    private void FixedUpdate()
    {
        // Update physics in this callback to avoid jittering!
        // Applying forces to the plane
        rb.AddForce(maxThrust * throttle * transform.up);

        rb.AddTorque(pitch * responsiveness * transform.right);
        rb.AddTorque(-roll * responsiveness * transform.forward);
        rb.AddTorque(yaw * responsiveness * transform.up);
    }
    private void HandleInput()
    {
        // Setting rotational values from axis inputs
        roll = Input.GetAxis("Roll");
        pitch = Input.GetAxis("Pitch");
        yaw = Input.GetAxis("Yaw");

        // Handling throttle inputs
        if (Input.GetKey(KeyCode.W))
        {
            throttle += throttleIncrement;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            throttle -= throttleIncrement;
        }
        throttle = Mathf.Clamp(throttle, 0f, 100f);
    }
}
