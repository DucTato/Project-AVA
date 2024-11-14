using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    // Adding a PlayerController Singleton
    public static PlayerController instance;
    public UIManager hudController;
    [SerializeField]
    private PlaneHandler planeHandler;
    [SerializeField]
    private HeliHandler heliHandler;
    [SerializeField]
    private FCS fireControl;
    
    [SerializeField]
    private new Camera camera;
    [SerializeField]
    private CinemachineFreeLook freeLookCam;
    private GamepadControls gpControls;
    [SerializeField]
    private Vector3 controlInput;
    private Vector2 stickValue;
    private AIController autoPilot;
    public int PlayerID { get; private set; }
    private void Awake()
    {
        instance = this;

        gpControls = new GamepadControls();
        gpControls.Gameplay.ACmovement.performed += context => { 
            stickValue = context.ReadValue<Vector2>();
            controlInput.x = stickValue.y;
            controlInput.z = -stickValue.x;
        };
        

        gpControls.Gameplay.moveCam.performed += context =>
        {
            if (context.ReadValue<Vector2>() != Vector2.zero)
            {
                freeLookCam.gameObject.SetActive(true);
                //Debug.Log("Look cam ON");
            }
            else
            {
                freeLookCam.gameObject.SetActive(false);
            }
        };

        gpControls.Gameplay.ACyaw.performed += context => {
            
            controlInput.y = context.ReadValue<float>();
        };
        gpControls.Gameplay.ACyaw.canceled += context => { 
            controlInput.y = 0;
        };

        gpControls.Gameplay.ACthrottle.performed += context => { if (planeHandler != null) planeHandler.SetThrottleInput(context.ReadValue<float>()); };
        gpControls.Gameplay.ACthrottle.canceled += context => { if (planeHandler != null) planeHandler.SetThrottleInput(0f); };

        gpControls.Gameplay.ACfireGUN.performed += context => { fireControl.FireCannon(true); };
        gpControls.Gameplay.ACfireGUN.canceled += context => { fireControl.FireCannon(false); };

        gpControls.Gameplay.ACfireMSL.performed += context => { fireControl.TryFireMissile(); };

        gpControls.Gameplay.ACcycleTgt.performed += context => {
            if (context.duration < 0.8f) fireControl.CycleTarget();
            else
            {
                if (fireControl.currTarget == null) return;
                freeLookCam.gameObject.SetActive(true);
                freeLookCam.m_LookAt = fireControl.currTarget.transform;
                //Debug.Log("Following current target");
            }
        };
        gpControls.Gameplay.ACcycleTgt.canceled += context => {
            freeLookCam.gameObject.SetActive(false);
            freeLookCam.m_LookAt = transform;
        };
    }
    // Start is called before the first frame update
    void Start()
    {
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        hudController = GameObject.FindGameObjectWithTag("UICanvas").GetComponent<UIManager>();
        SetPlane(planeHandler);
        PlayerID = gameObject.GetInstanceID();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInputs();
    }
    private void SetPlane(PlaneHandler plane)
    {
        this.planeHandler = plane;
        autoPilot = plane.GetComponent<AIController>();

        if (hudController != null)
        {
            hudController.SetPlane(plane);
            hudController.SetCamera(camera);
        }

        //planeCamera.SetPlane(plane);
    }
    private void HandleInputs()
    {
        if (Input.GetKeyDown(KeyCode.Space)) autoPilot.enabled = !autoPilot.enabled;
        
        if (planeHandler != null)
        {
            if (autoPilot.enabled) return;
            planeHandler.SetControlInput(controlInput);
        }
    }
    private void OnEnable()
    {
        gpControls.Gameplay.Enable();
    }
    private void OnDisable()
    {
        gpControls.Gameplay.Disable();
    }
    public bool CheckIsPlayer(GameObject go)
    {
        if (go.GetInstanceID() == PlayerID) return true;
        else return false; 
    }
}
