using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _2048
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; } = null;

        private IA_2048 InpAct;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else 
                Destroy(Instance);

            InpAct = new();

            InpAct.Enable();
            InpAct.Swipes.Enable();
        }

        public float TickHorizontalSwipe()
        {
            return InpAct.Swipes.LeftRight.ReadValue<float>();
        }
        public float TickVerticalSwipe()
        {
            return InpAct.Swipes.UpDown.ReadValue<float>();
        }

        public void SwipesSleepTimer(float time)
        {
            InpAct.Swipes.Disable();
            Invoke(nameof(ReEnableSwipes), time);
        }

        private void ReEnableSwipes()
        {
            InpAct.Swipes.Enable();
        }

        private IEnumerator ReEnableInputAction(InputAction inputAction, float time)
        {
            yield return new WaitForSeconds(time);
            inputAction.Enable();
        }
    }
}