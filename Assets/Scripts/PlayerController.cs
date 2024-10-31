using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Adding a PlayerController Singleton
    public static PlayerController instance;
    [SerializeField]
    private PlaneHandler planeHandler;
    [SerializeField]
    private HeliHandler heliHandler;
    [SerializeField]
    private FCS fireControl;
    private GamepadControls gpControls;
    [SerializeField]
    private Vector3 controlInput;
    private Vector2 stickValue;
    private void Awake()
    {
        instance = this;
        gpControls = new GamepadControls();
        gpControls.Gameplay.ACmovement.performed += context => { 
            stickValue = context.ReadValue<Vector2>();
            controlInput.x = stickValue.y;
            controlInput.z = -stickValue.x;
        };
        gpControls.Gameplay.ACmovement.canceled += context => { stickValue = Vector2.zero;
            controlInput = Vector3.zero;
        };

        gpControls.Gameplay.ACyaw.performed += context => {
            
            controlInput.y = context.ReadValue<float>();
        };
        gpControls.Gameplay.ACyaw.canceled += context => { 
            controlInput.y = 0;
        };

        gpControls.Gameplay.ACthrottle.performed += context => { if (planeHandler != null) planeHandler.SetThrottleInput(context.ReadValue<float>()); };

        gpControls.Gameplay.ACfireGUN.performed += context => { fireControl.FireCannon(true); };
        gpControls.Gameplay.ACfireGUN.canceled += context => { fireControl.FireCannon(false); };

        gpControls.Gameplay.ACfireMSL.performed += context => { fireControl.TryFireMissile(); };
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleInputs();
    }
    private void HandleInputs()
    {
        if (planeHandler != null)
        {
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
}
