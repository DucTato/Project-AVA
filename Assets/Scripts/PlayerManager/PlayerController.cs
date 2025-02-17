using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class PlayerController : MonoBehaviour
{
    // Adding a PlayerController Singleton
    public static PlayerController instance;
    public UIManager hudController;
    [SerializeField]
    private PlaneHandler planeHandler;
    [SerializeField]
    private HeliHandler heliHandler;
    
    public FCS fireControl;
    
    [SerializeField]
    private new Camera camera;
    [SerializeField]
    private CinemachineFreeLook freeLookCam;
    
    public PlayerInput playerInput;
    private Vector3 controlInput;
    private AIController autoPilot;
    private string[] cheatCode;
    private int codeIndex;
    public int PlayerID { get; private set; }
    #region CallBacks
    private void Awake()
    {
        instance = this;
        playerInput = GetComponent<PlayerInput>();
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }
    // Start is called before the first frame update
    void Start()
    {
        SetCurrentInputMap("UI");
        cheatCode = new string[] { "m", "a", "v", "e", "r", "i", "c", "k"};  // the cheat code is: MAVERICK

        //Debug.Log(playerInput.actions.FindActionMap("Gameplay").enabled);
    }

    // Update is called once per frame
    void Update()
    {
        HandleInputs();
        //playerInput.actions.FindActionMap("Gameplay").Disable();
    }
    private void OnDestroy()
    {
        fireControl.AmmoUpdate -= OnAmmoUpdate;
    }
    #endregion
    #region Input Handler
    public void OnPauseButton(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
        {
            //Debug.Log("Player Controller called");
            hudController.TogglePause();
        }
    }
    public void OnMoveCam(InputAction.CallbackContext ctx)
    {
        if (ctx.ReadValue<Vector2>() != Vector2.zero)
        {
            // Free move cam ON
            freeLookCam.Priority = 100;
        }
        else
        {
            // Free move cam OFF
            freeLookCam.Priority = 0;
            
        }
    }
    public void OnThrottleInput(InputAction.CallbackContext ctx)
    {
        if (planeHandler == null) return;
        if (autoPilot.enabled) return;

        planeHandler.SetThrottleInput(ctx.ReadValue<float>());
    }
    public void OnRollPitchInput(InputAction.CallbackContext ctx)
    {
        if (planeHandler == null) return;
        var input = ctx.ReadValue<Vector2>();
        controlInput = new Vector3(input.y, controlInput.y, -input.x);
    }
    public void OnYawInput(InputAction.CallbackContext ctx)
    {
        if (planeHandler == null) return;
        var input = ctx.ReadValue<float>();
        controlInput = new Vector3(controlInput.x, input, controlInput.z);
    }
    public void OnCycleTarget(InputAction.CallbackContext ctx)
    {
        // 2 different function are bound to the same key, which are activated by either holding down or tapping the button
        if (planeHandler == null) return;
        if (ctx.phase == InputActionPhase.Performed)
        {
            if (ctx.duration < 0.8f) fireControl.CycleTarget();
            else
            {
                if (fireControl.currTarget == null) return;
                //freeLookCam.gameObject.SetActive(true);
                freeLookCam.Priority = 100;
                freeLookCam.m_LookAt = fireControl.currTarget.transform;
                //Debug.Log("Following current target");
            }
        }
        if (ctx.phase == InputActionPhase.Canceled)
        {
            freeLookCam.Priority = 0;
            //freeLookCam.gameObject.SetActive(false);
            freeLookCam.m_LookAt = transform;
        }
    }
    public void OnFireCannon(InputAction.CallbackContext ctx)
    {
        if (planeHandler == null) return;
        if (ctx.phase == InputActionPhase.Performed)
        {
            fireControl.FireCannon(true);
        }
        if (ctx.phase == InputActionPhase.Canceled)
        {
            fireControl.FireCannon(false);
        }
    }
    public void OnFireMissile(InputAction.CallbackContext ctx)
    {
        if (planeHandler == null) return;
        if (ctx.phase == InputActionPhase.Performed)
        {
            fireControl.TryFireMissile();
        }
    }
    private void HandleInputs()
    {
        // Cheat code detection
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(cheatCode[codeIndex]))
            {
                codeIndex++;
            }
            else
            {
                codeIndex = 0; // If the sequence is interrupted by a wrong input, reset the input sequence
            }
            if (codeIndex == cheatCode.Length)
            {
                Debug.Log("Cheat Code Activated");
                ToggleAi();
                codeIndex = 0; // Reset the input sequence
            }
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            //Debug.Log("Toggle UI");
            //Toggle UI with space
            hudController.ToggleCanvas();
        }
        if (planeHandler != null)
        {
            if (autoPilot.enabled) return;
            planeHandler.SetControlInput(controlInput);
        }
    }
    #endregion
    #region Procedures
    private void OnAmmoUpdate(object sender, EventArgs e)
    {
        hudController.UpdateAmmoHUD(fireControl.CurrentCannonAmmo, fireControl.CurrentMissileAmmo, fireControl.CurrentMissileStorage);
    }
    public bool CheckIsPlayer(GameObject go)
    {
        if (go.GetInstanceID() == PlayerID) return true;
        else return false; 
    }
    public void SetUpPlayer(GameObject obj)
    {
        planeHandler = obj.GetComponent<PlaneHandler>();
        freeLookCam = obj.GetComponentInChildren<CinemachineFreeLook>();
        autoPilot = obj.GetComponent<AIController>();
        hudController = GameManager.instance.hudController;
        hudController.SetPlane(planeHandler);
        fireControl = obj.GetComponent<FCS>();
        //
        fireControl.AmmoUpdate += OnAmmoUpdate;
        //
        hudController.SetCamera(camera);
        PlayerID = obj.GetInstanceID();
    }
    public void ToggleAi()
    {
        autoPilot.enabled = !autoPilot.enabled;
    }
    public void ToggleAi(bool value)
    {
        autoPilot.enabled = value;
    }
    public void SetCurrentInputMap(string actionMapName)
    {
        
        playerInput.SwitchCurrentActionMap(actionMapName);
        playerInput.currentActionMap.Enable();
    }
    public PlaneHandler GetPlayerAircraft()
    {
        return planeHandler;
    }
    public FCS GetPlayerFCS()
    {
        return fireControl;
    }
    #endregion
}
