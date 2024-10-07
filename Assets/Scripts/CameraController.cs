using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private GamepadControls gpControls;
    [SerializeField]
    [Tooltip("An array of transforms containing the camera positions")]
    private Transform[] povs;
    [SerializeField]
    private float catchSpeed = 10f;
    private int camIndex = 0;
    private Vector3 target;

    private void Awake()
    {
        gpControls = new GamepadControls();
        gpControls.Gameplay.toggleCam.performed += context =>
        {
            // Cycling through the array of POVs
            camIndex++;
            if (camIndex >= povs.Length)
            {
                camIndex = 0;
            }
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
        povs = PlayerController.instance.camPovs;
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
        transform.forward = povs[camIndex].forward;
        //Debug.Log("Camera culling");
    }
}
