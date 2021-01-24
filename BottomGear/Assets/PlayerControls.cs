// GENERATED AUTOMATICALLY FROM 'Assets/PlayerControls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerControls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerControls"",
    ""maps"": [
        {
            ""name"": ""Gameplay"",
            ""id"": ""81c65294-2294-48d1-86a2-35f3673ff27d"",
            ""actions"": [
                {
                    ""name"": ""Orientation"",
                    ""type"": ""PassThrough"",
                    ""id"": ""bba589eb-3ab1-4057-bc33-e3ee2fd0a164"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Acceleration"",
                    ""type"": ""Value"",
                    ""id"": ""db6b51fb-b0aa-40d3-94d6-093f7f3eb29d"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Deceleration"",
                    ""type"": ""Value"",
                    ""id"": ""d0e2fc87-597a-4733-a192-83041bdc233b"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Throw"",
                    ""type"": ""Button"",
                    ""id"": ""5685df62-e855-46ec-ac2d-96a75b04e921"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Shoot"",
                    ""type"": ""Value"",
                    ""id"": ""77f32e8e-06e8-4505-b4cb-707728c77a06"",
                    ""expectedControlType"": ""Integer"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""3ef90d9e-d7cb-4e47-ad18-dee61ebbdcdd"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": ""StickDeadzone(min=0.19)"",
                    ""groups"": ""Controls"",
                    ""action"": ""Orientation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4573f44f-71a9-4060-8dc0-c979316f3e06"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": ""AxisDeadzone(min=0.19)"",
                    ""groups"": ""Controls"",
                    ""action"": ""Acceleration"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""be2f3c1f-6cd7-470a-ba76-0578896f570b"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": ""AxisDeadzone(min=0.19)"",
                    ""groups"": ""Controls"",
                    ""action"": ""Deceleration"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ee893750-127e-4487-a7b0-0fa49a077007"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Controls"",
                    ""action"": ""Throw"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""046195f3-6122-4f37-b8fe-a11649568cc8"",
                    ""path"": ""<Gamepad>/dpad/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Controls"",
                    ""action"": ""Throw"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e1739fde-c9f7-4685-825a-e4cff43d922d"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Controls"",
                    ""action"": ""Shoot"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6c4c7419-ca5f-4684-95df-1e871d88e228"",
                    ""path"": ""<Gamepad>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Controls"",
                    ""action"": ""Shoot"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Controls"",
            ""bindingGroup"": ""Controls"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": true,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": true,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Gamepad>"",
                    ""isOptional"": true,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Gameplay
        m_Gameplay = asset.FindActionMap("Gameplay", throwIfNotFound: true);
        m_Gameplay_Orientation = m_Gameplay.FindAction("Orientation", throwIfNotFound: true);
        m_Gameplay_Acceleration = m_Gameplay.FindAction("Acceleration", throwIfNotFound: true);
        m_Gameplay_Deceleration = m_Gameplay.FindAction("Deceleration", throwIfNotFound: true);
        m_Gameplay_Throw = m_Gameplay.FindAction("Throw", throwIfNotFound: true);
        m_Gameplay_Shoot = m_Gameplay.FindAction("Shoot", throwIfNotFound: true);
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

    // Gameplay
    private readonly InputActionMap m_Gameplay;
    private IGameplayActions m_GameplayActionsCallbackInterface;
    private readonly InputAction m_Gameplay_Orientation;
    private readonly InputAction m_Gameplay_Acceleration;
    private readonly InputAction m_Gameplay_Deceleration;
    private readonly InputAction m_Gameplay_Throw;
    private readonly InputAction m_Gameplay_Shoot;
    public struct GameplayActions
    {
        private @PlayerControls m_Wrapper;
        public GameplayActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Orientation => m_Wrapper.m_Gameplay_Orientation;
        public InputAction @Acceleration => m_Wrapper.m_Gameplay_Acceleration;
        public InputAction @Deceleration => m_Wrapper.m_Gameplay_Deceleration;
        public InputAction @Throw => m_Wrapper.m_Gameplay_Throw;
        public InputAction @Shoot => m_Wrapper.m_Gameplay_Shoot;
        public InputActionMap Get() { return m_Wrapper.m_Gameplay; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(GameplayActions set) { return set.Get(); }
        public void SetCallbacks(IGameplayActions instance)
        {
            if (m_Wrapper.m_GameplayActionsCallbackInterface != null)
            {
                @Orientation.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnOrientation;
                @Orientation.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnOrientation;
                @Orientation.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnOrientation;
                @Acceleration.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnAcceleration;
                @Acceleration.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnAcceleration;
                @Acceleration.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnAcceleration;
                @Deceleration.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnDeceleration;
                @Deceleration.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnDeceleration;
                @Deceleration.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnDeceleration;
                @Throw.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnThrow;
                @Throw.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnThrow;
                @Throw.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnThrow;
                @Shoot.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnShoot;
                @Shoot.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnShoot;
                @Shoot.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnShoot;
            }
            m_Wrapper.m_GameplayActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Orientation.started += instance.OnOrientation;
                @Orientation.performed += instance.OnOrientation;
                @Orientation.canceled += instance.OnOrientation;
                @Acceleration.started += instance.OnAcceleration;
                @Acceleration.performed += instance.OnAcceleration;
                @Acceleration.canceled += instance.OnAcceleration;
                @Deceleration.started += instance.OnDeceleration;
                @Deceleration.performed += instance.OnDeceleration;
                @Deceleration.canceled += instance.OnDeceleration;
                @Throw.started += instance.OnThrow;
                @Throw.performed += instance.OnThrow;
                @Throw.canceled += instance.OnThrow;
                @Shoot.started += instance.OnShoot;
                @Shoot.performed += instance.OnShoot;
                @Shoot.canceled += instance.OnShoot;
            }
        }
    }
    public GameplayActions @Gameplay => new GameplayActions(this);
    private int m_ControlsSchemeIndex = -1;
    public InputControlScheme ControlsScheme
    {
        get
        {
            if (m_ControlsSchemeIndex == -1) m_ControlsSchemeIndex = asset.FindControlSchemeIndex("Controls");
            return asset.controlSchemes[m_ControlsSchemeIndex];
        }
    }
    public interface IGameplayActions
    {
        void OnOrientation(InputAction.CallbackContext context);
        void OnAcceleration(InputAction.CallbackContext context);
        void OnDeceleration(InputAction.CallbackContext context);
        void OnThrow(InputAction.CallbackContext context);
        void OnShoot(InputAction.CallbackContext context);
    }
}
