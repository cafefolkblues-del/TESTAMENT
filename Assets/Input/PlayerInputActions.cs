// Auto-generated style Input Actions wrapper
// New Input System 콜백 기반 입력 처리용
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class PlayerInputActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }

    public PlayerInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
            ""name"": ""PlayerInputActions"",
            ""maps"": [
                {
                    ""name"": ""Player"",
                    ""id"": ""a1b2c3d4-e5f6-7890-abcd-ef1234567890"",
                    ""actions"": [
                        {
                            ""name"": ""Move"",
                            ""type"": ""Value"",
                            ""id"": ""11111111-1111-1111-1111-111111111111"",
                            ""expectedControlType"": ""Axis"",
                            ""processors"": """",
                            ""interactions"": """"
                        },
                        {
                            ""name"": ""Jump"",
                            ""type"": ""Button"",
                            ""id"": ""22222222-2222-2222-2222-222222222222"",
                            ""expectedControlType"": ""Button"",
                            ""processors"": """",
                            ""interactions"": """"
                        },
                        {
                            ""name"": ""Fire"",
                            ""type"": ""Button"",
                            ""id"": ""33333333-3333-3333-3333-333333333333"",
                            ""expectedControlType"": ""Button"",
                            ""processors"": """",
                            ""interactions"": """"
                        },
                        {
                            ""name"": ""AltFire"",
                            ""type"": ""Button"",
                            ""id"": ""44444444-4444-4444-4444-444444444444"",
                            ""expectedControlType"": ""Button"",
                            ""processors"": """",
                            ""interactions"": """"
                        }
                    ],
                    ""bindings"": [
                        {
                            ""name"": ""AD"",
                            ""id"": ""55555555-5555-5555-5555-555555555551"",
                            ""path"": ""1DAxis"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""groups"": """",
                            ""action"": ""Move"",
                            ""isComposite"": true,
                            ""isPartOfComposite"": false
                        },
                        {
                            ""name"": ""negative"",
                            ""id"": ""55555555-5555-5555-5555-555555555552"",
                            ""path"": ""<Keyboard>/a"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""groups"": """",
                            ""action"": ""Move"",
                            ""isComposite"": false,
                            ""isPartOfComposite"": true
                        },
                        {
                            ""name"": ""positive"",
                            ""id"": ""55555555-5555-5555-5555-555555555553"",
                            ""path"": ""<Keyboard>/d"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""groups"": """",
                            ""action"": ""Move"",
                            ""isComposite"": false,
                            ""isPartOfComposite"": true
                        },
                        {
                            ""name"": ""Arrows"",
                            ""id"": ""55555555-5555-5555-5555-555555555554"",
                            ""path"": ""1DAxis"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""groups"": """",
                            ""action"": ""Move"",
                            ""isComposite"": true,
                            ""isPartOfComposite"": false
                        },
                        {
                            ""name"": ""negative"",
                            ""id"": ""55555555-5555-5555-5555-555555555555"",
                            ""path"": ""<Keyboard>/leftArrow"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""groups"": """",
                            ""action"": ""Move"",
                            ""isComposite"": false,
                            ""isPartOfComposite"": true
                        },
                        {
                            ""name"": ""positive"",
                            ""id"": ""55555555-5555-5555-5555-555555555556"",
                            ""path"": ""<Keyboard>/rightArrow"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""groups"": """",
                            ""action"": ""Move"",
                            ""isComposite"": false,
                            ""isPartOfComposite"": true
                        },
                        {
                            ""name"": """",
                            ""id"": ""66666666-6666-6666-6666-666666666661"",
                            ""path"": ""<Keyboard>/w"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""groups"": """",
                            ""action"": ""Jump"",
                            ""isComposite"": false,
                            ""isPartOfComposite"": false
                        },
                        {
                            ""name"": """",
                            ""id"": ""66666666-6666-6666-6666-666666666662"",
                            ""path"": ""<Keyboard>/space"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""groups"": """",
                            ""action"": ""Jump"",
                            ""isComposite"": false,
                            ""isPartOfComposite"": false
                        },
                        {
                            ""name"": """",
                            ""id"": ""77777777-7777-7777-7777-777777777771"",
                            ""path"": ""<Mouse>/leftButton"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""groups"": """",
                            ""action"": ""Fire"",
                            ""isComposite"": false,
                            ""isPartOfComposite"": false
                        },
                        {
                            ""name"": """",
                            ""id"": ""88888888-8888-8888-8888-888888888881"",
                            ""path"": ""<Mouse>/rightButton"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""groups"": """",
                            ""action"": ""AltFire"",
                            ""isComposite"": false,
                            ""isPartOfComposite"": false
                        }
                    ]
                }
            ],
            ""controlSchemes"": []
        }");

        _player = new PlayerActions(asset);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    // IInputActionCollection 구현
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

    public bool Contains(InputAction action) => asset.Contains(action);
    public IEnumerator<InputAction> GetEnumerator() => asset.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    public void Enable() => asset.Enable();
    public void Disable() => asset.Disable();

    // Player Action Map
    private PlayerActions _player;
    public PlayerActions Player => _player;

    public class PlayerActions
    {
        private InputActionMap _map;
        private InputAction _move;
        private InputAction _jump;
        private InputAction _fire;
        private InputAction _altFire;

        public PlayerActions(InputActionAsset asset)
        {
            _map = asset.FindActionMap("Player", throwIfNotFound: true);
            _move = _map.FindAction("Move", throwIfNotFound: true);
            _jump = _map.FindAction("Jump", throwIfNotFound: true);
            _fire = _map.FindAction("Fire", throwIfNotFound: true);
            _altFire = _map.FindAction("AltFire", throwIfNotFound: true);
        }

        public InputAction Move => _move;
        public InputAction Jump => _jump;
        public InputAction Fire => _fire;
        public InputAction AltFire => _altFire;

        public void Enable() => _map.Enable();
        public void Disable() => _map.Disable();
    }
}
