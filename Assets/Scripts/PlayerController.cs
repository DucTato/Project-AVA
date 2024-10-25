using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Adding a PlayerController Singleton
    public static PlayerController instance;
    private GamepadControls gpControls;
    private Vector2 stickValue;
    private void Awake()
    {
        instance = this;
        gpControls = new GamepadControls();
        gpControls.Gameplay.ACmovement.performed += context => stickValue = context.ReadValue<Vector2>();
        gpControls.Gameplay.ACmovement.canceled += context => stickValue = Vector2.zero;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void HandleInputs()
    {

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
