using UnityEngine;

namespace JohnStairs.RCC.Inputs {
    public class RPGInputManager {
        private static RPGInputActions _inputActions;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init() {
            _inputActions = null;
        }

        public static RPGInputActions GetInputActions() {
            if (_inputActions == null) {
                _inputActions = new RPGInputActions();
                _inputActions.Enable();
            }

            return _inputActions;
        }

        public static void DisableInputActions() {
            if (_inputActions != null) {
                _inputActions.Disable();
            }
        }

        public static void EnableInputActions() {
            if (_inputActions != null) {
                _inputActions.Enable();
            }
        }
    }
}
