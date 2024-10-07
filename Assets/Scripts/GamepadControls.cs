//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.11.1
//     from Assets/PlayerControls.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @GamepadControls: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @GamepadControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerControls"",
    ""maps"": [
        {
            ""name"": ""Gameplay"",
            ""id"": ""ceb48719-b664-4442-b0c7-d2b4d24d58a9"",
            ""actions"": [
                {
                    ""name"": ""ACmovement"",
                    ""type"": ""PassThrough"",
                    ""id"": ""286b220f-5f12-48f1-8b9e-91bf7b641e43"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ACyaw"",
                    ""type"": ""Value"",
                    ""id"": ""f54342a2-601c-417c-9046-e5bc8d4aebc4"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""ACthrottle"",
                    ""type"": ""PassThrough"",
                    ""id"": ""636f1987-e23c-410a-bf20-f560c1dd3d94"",
                    ""expectedControlType"": ""Analog"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""moveCam"",
                    ""type"": ""PassThrough"",
                    ""id"": ""781357ee-8172-4e5f-a50f-a7e53e5aa84a"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""toggleCam"",
                    ""type"": ""Button"",
                    ""id"": ""89bb7773-d740-4aa4-b431-6fbebe6884e9"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""ca8993c0-8f24-4de5-827b-35d7adbabdbc"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";DualStickController"",
                    ""action"": ""ACmovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""23e8e495-2180-4bb6-aa19-bcfc71722035"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";DualStickController"",
                    ""action"": ""moveCam"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""bf7f4f1b-a769-455a-a174-62a402be7032"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ACyaw"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""2be54c3d-1a8d-4b6c-bcf3-6cc26d729b96"",
                    ""path"": ""<Gamepad>/leftShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";DualStickController"",
                    ""action"": ""ACyaw"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""1dd4ec88-97a8-4b6f-bf44-6c4350590ee6"",
                    ""path"": ""<Gamepad>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";DualStickController"",
                    ""action"": ""ACyaw"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""778b41d3-19c1-4fcf-8e00-1dd1ed0d79bd"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ACthrottle"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""346aec30-0af4-4d44-ae71-0322ebae49ef"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";DualStickController"",
                    ""action"": ""ACthrottle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""f78bcf33-7076-426f-85df-eef1306a0ddb"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";DualStickController"",
                    ""action"": ""ACthrottle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""c10fbac4-b85f-46f1-91ca-75e2dbfa7619"",
                    ""path"": ""<Gamepad>/select"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";DualStickController"",
                    ""action"": ""toggleCam"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""DualStickController"",
            ""bindingGroup"": ""DualStickController"",
            ""devices"": [
                {
                    ""devicePath"": ""<Gamepad>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""KB&M"",
            ""bindingGroup"": ""KB&M"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Gameplay
        m_Gameplay = asset.FindActionMap("Gameplay", throwIfNotFound: true);
        m_Gameplay_ACmovement = m_Gameplay.FindAction("ACmovement", throwIfNotFound: true);
        m_Gameplay_ACyaw = m_Gameplay.FindAction("ACyaw", throwIfNotFound: true);
        m_Gameplay_ACthrottle = m_Gameplay.FindAction("ACthrottle", throwIfNotFound: true);
        m_Gameplay_moveCam = m_Gameplay.FindAction("moveCam", throwIfNotFound: true);
        m_Gameplay_toggleCam = m_Gameplay.FindAction("toggleCam", throwIfNotFound: true);
    }

    ~@GamepadControls()
    {
        UnityEngine.Debug.Assert(!m_Gameplay.enabled, "This will cause a leak and performance issues, GamepadControls.Gameplay.Disable() has not been called.");
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Gameplay
    private readonly InputActionMap m_Gameplay;
    private List<IGameplayActions> m_GameplayActionsCallbackInterfaces = new List<IGameplayActions>();
    private readonly InputAction m_Gameplay_ACmovement;
    private readonly InputAction m_Gameplay_ACyaw;
    private readonly InputAction m_Gameplay_ACthrottle;
    private readonly InputAction m_Gameplay_moveCam;
    private readonly InputAction m_Gameplay_toggleCam;
    public struct GameplayActions
    {
        private @GamepadControls m_Wrapper;
        public GameplayActions(@GamepadControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @ACmovement => m_Wrapper.m_Gameplay_ACmovement;
        public InputAction @ACyaw => m_Wrapper.m_Gameplay_ACyaw;
        public InputAction @ACthrottle => m_Wrapper.m_Gameplay_ACthrottle;
        public InputAction @moveCam => m_Wrapper.m_Gameplay_moveCam;
        public InputAction @toggleCam => m_Wrapper.m_Gameplay_toggleCam;
        public InputActionMap Get() { return m_Wrapper.m_Gameplay; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(GameplayActions set) { return set.Get(); }
        public void AddCallbacks(IGameplayActions instance)
        {
            if (instance == null || m_Wrapper.m_GameplayActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_GameplayActionsCallbackInterfaces.Add(instance);
            @ACmovement.started += instance.OnACmovement;
            @ACmovement.performed += instance.OnACmovement;
            @ACmovement.canceled += instance.OnACmovement;
            @ACyaw.started += instance.OnACyaw;
            @ACyaw.performed += instance.OnACyaw;
            @ACyaw.canceled += instance.OnACyaw;
            @ACthrottle.started += instance.OnACthrottle;
            @ACthrottle.performed += instance.OnACthrottle;
            @ACthrottle.canceled += instance.OnACthrottle;
            @moveCam.started += instance.OnMoveCam;
            @moveCam.performed += instance.OnMoveCam;
            @moveCam.canceled += instance.OnMoveCam;
            @toggleCam.started += instance.OnToggleCam;
            @toggleCam.performed += instance.OnToggleCam;
            @toggleCam.canceled += instance.OnToggleCam;
        }

        private void UnregisterCallbacks(IGameplayActions instance)
        {
            @ACmovement.started -= instance.OnACmovement;
            @ACmovement.performed -= instance.OnACmovement;
            @ACmovement.canceled -= instance.OnACmovement;
            @ACyaw.started -= instance.OnACyaw;
            @ACyaw.performed -= instance.OnACyaw;
            @ACyaw.canceled -= instance.OnACyaw;
            @ACthrottle.started -= instance.OnACthrottle;
            @ACthrottle.performed -= instance.OnACthrottle;
            @ACthrottle.canceled -= instance.OnACthrottle;
            @moveCam.started -= instance.OnMoveCam;
            @moveCam.performed -= instance.OnMoveCam;
            @moveCam.canceled -= instance.OnMoveCam;
            @toggleCam.started -= instance.OnToggleCam;
            @toggleCam.performed -= instance.OnToggleCam;
            @toggleCam.canceled -= instance.OnToggleCam;
        }

        public void RemoveCallbacks(IGameplayActions instance)
        {
            if (m_Wrapper.m_GameplayActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IGameplayActions instance)
        {
            foreach (var item in m_Wrapper.m_GameplayActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_GameplayActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public GameplayActions @Gameplay => new GameplayActions(this);
    private int m_DualStickControllerSchemeIndex = -1;
    public InputControlScheme DualStickControllerScheme
    {
        get
        {
            if (m_DualStickControllerSchemeIndex == -1) m_DualStickControllerSchemeIndex = asset.FindControlSchemeIndex("DualStickController");
            return asset.controlSchemes[m_DualStickControllerSchemeIndex];
        }
    }
    private int m_KBMSchemeIndex = -1;
    public InputControlScheme KBMScheme
    {
        get
        {
            if (m_KBMSchemeIndex == -1) m_KBMSchemeIndex = asset.FindControlSchemeIndex("KB&M");
            return asset.controlSchemes[m_KBMSchemeIndex];
        }
    }
    public interface IGameplayActions
    {
        void OnACmovement(InputAction.CallbackContext context);
        void OnACyaw(InputAction.CallbackContext context);
        void OnACthrottle(InputAction.CallbackContext context);
        void OnMoveCam(InputAction.CallbackContext context);
        void OnToggleCam(InputAction.CallbackContext context);
    }
}
