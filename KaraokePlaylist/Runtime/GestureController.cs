
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Playlist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class GestureController : UdonSharpBehaviour
    {
        public FloatUIController floatUIController;

        public float triggerDelay = 0.5f;

        private float triggerDuration = 0f;
        private bool currentLeftUseValue = false;
        private bool currentRightUseValue = false;

        void Start()
        {

        }

        void Update()
        {
            float newTriggerDuration = 0;
            if (!floatUIController.IsHeld)
            {
                if (Input.GetKey(KeyCode.E) || (currentLeftUseValue && currentRightUseValue))
                    newTriggerDuration = triggerDuration + Time.deltaTime;
                else
                    newTriggerDuration = 0f;
            }

            if (newTriggerDuration > triggerDelay && triggerDuration < triggerDelay)
                floatUIController.OnToggle();

            triggerDuration = newTriggerDuration;
        }
        
        public override void InputUse(bool value, VRC.Udon.Common.UdonInputEventArgs args) {
            if (InputManager.IsUsingHandController())
            {
                if (args.handType == VRC.Udon.Common.HandType.LEFT)
                    currentLeftUseValue = value;
                else if (args.handType == VRC.Udon.Common.HandType.RIGHT)
                    currentRightUseValue = value;
            }
            else
            {
                currentLeftUseValue = false;
                currentRightUseValue = false;
            }
        }
    }
}