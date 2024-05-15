using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Input Manager", menuName = "Manager/Input Manager")]
    public class InputManagerSO : ScriptableObject
    {
        [Header("Event Channels")]
        [SerializeField] VoidEventChannelSO beginGameEventChannel;
        [SerializeField] VoidEventChannelSO pauseEventChannel;
        [SerializeField] BoolEventChannelSO radioEventChannel;

        ActionMaps actMap;
        InputAction move_de_Accelerate;
        InputAction move_changeLane;
        InputAction menu_pause;
        InputAction menu_radio;

        public event UnityAction<int, bool> OnDe_Accelerate;
        public event UnityAction<int, bool> OnChangeLane;

        void OnEnable()
        {
            actMap = new ActionMaps();
            move_de_Accelerate = actMap.Move.De_Accelerate;
            move_de_Accelerate.started += OnDe_AccelerateRaised;
            move_de_Accelerate.canceled += OnDe_AccelerateRaised;

            move_changeLane = actMap.Move.ChangeLane;
            move_changeLane.started += OnChangeLaneRaised;
            move_changeLane.canceled += OnChangeLaneRaised;

            menu_radio = actMap.Menu.Radio;
            menu_radio.started += OnRadioRaised;
            menu_radio.canceled += OnRadioRaised;
            menu_radio.Enable();

            menu_pause = actMap.Menu.Pause;
            menu_pause.started += OnPauseRaised;

            if (beginGameEventChannel != null)
                beginGameEventChannel.OnEventRaised += BeginGame;
        }

        void OnDisable()
        {
            if (move_de_Accelerate.enabled)
                move_de_Accelerate.Disable();
            if (move_changeLane.enabled)
                move_changeLane.Disable();
            if (menu_radio.enabled)
                menu_radio.Disable();
            if (menu_pause.enabled)
                menu_pause.Disable();
        }

        void BeginGame()
        {
            menu_pause.Enable();
            move_de_Accelerate.Enable();
            move_changeLane.Enable();
        }

        void OnDe_AccelerateRaised(InputAction.CallbackContext callbackContext)
        {   
            if (OnDe_Accelerate != null)
            {
                bool buttonIsPressed = callbackContext.started;
                float rawInput = callbackContext.ReadValue<float>();
                if (rawInput != 0)
                {
                    int direction = rawInput > 0 ? 1 : -1;
                    OnDe_Accelerate.Invoke(direction, buttonIsPressed);
                    return;
                }
                OnDe_Accelerate.Invoke(0, buttonIsPressed);
            }
            else
            {
                Debug.LogWarning("Input on (De-)Acceleration channel was detected but nobody listened");
            }
        }

        void OnChangeLaneRaised(InputAction.CallbackContext callbackContext)
        {
            if (OnChangeLane != null)
            {
                bool buttonIsPressed = callbackContext.started;
                float rawInput = callbackContext.ReadValue<float>();
                if (rawInput != 0)
                {
                    int direction = rawInput > 0 ? 1 : -1;
                    OnChangeLane.Invoke(direction, buttonIsPressed);
                    return;
                }
                OnChangeLane.Invoke(0, buttonIsPressed);
            }
            else
            {
                Debug.LogWarning("Input on ChangeLane channel was detected but nobody listened");
            }
        }

        void OnPauseRaised(InputAction.CallbackContext callbackContext)
        {
            pauseEventChannel.RaiseEvent();
        }

        void OnRadioRaised(InputAction.CallbackContext callbackContext)
        {
            radioEventChannel.RaiseEvent(callbackContext.started);
        }
    }
}

