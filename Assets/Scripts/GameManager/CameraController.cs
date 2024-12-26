using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    
    [SerializeField]
    [Tooltip("An array of transforms containing the camera positions")]
    private Transform[] povs;
    [SerializeField]
    private float catchSpeed = 10f;
    [SerializeField]
    private float lookSpeed = 10f;
    private int camIndex = 0;
    private Vector3 target;
    private Vector2 stickValue, rotation;
    private InputAction stickState;

    private void Awake()
    {
        //gpControls = new GamepadControls();
        //stickState = gpControls.Gameplay.moveCam;
        rotation = Vector2.zero;
        //gpControls.Gameplay.toggleCam.performed += context =>
        //{
        //    // Cycling through the array of POVs
        //    camIndex++;
        //    if (camIndex >= povs.Length)
        //    {
        //        camIndex = 0;
        //    }
        //};

        //gpControls.Gameplay.moveCam.performed += context => 
        //    {
        //        stickValue = context.ReadValue<Vector2>();
        //    };
        //gpControls.Gameplay.moveCam.canceled += context => stickValue = Vector2.zero;
    }
    private void OnEnable()
    {
        //gpControls.Gameplay.Enable();
    }
    private void OnDisable()
    {
        //gpControls.Gameplay.Disable();
    }
    // Start is called before the first frame update
    void Start()
    {
        // Grabbing the camera Position from the Player Controller singleton
        //povs = PlayerController.instance.camPovs;
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.V))          // Legacy Input manager system
        //{
        //    // Cycling through the array of POVs
        //    camIndex++;
        //    if (camIndex >= povs.Length)
        //    {
        //        camIndex = 0;
        //    }
        //}
        target = povs[camIndex].position;
    }
    private void FixedUpdate()
    {
        // Updating the camera position in this callback function to avoid jittering
        transform.position = Vector3.MoveTowards(transform.position, target, catchSpeed * Time.deltaTime);
        FreeLook();
        //Debug.Log("Camera culling");
    }

    private void FreeLook()
    {
        if (stickState.IsPressed())
        {
            rotation.x = -stickValue.y;
            rotation.y = stickValue.x;
            transform.Rotate(lookSpeed * Time.deltaTime * rotation, Space.Self);
        }
        else 
        {
            //transform.forward = Vector3.Lerp(transform.forward, povs[camIndex].forward, 3f * Time.deltaTime); 
            transform.rotation = Quaternion.Slerp(transform.rotation, povs[camIndex].rotation, 3f * Time.deltaTime);
        }
    }
}
