
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace RayCastUtils {

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RayCastScrollViewHelper : UdonSharpBehaviour
    {

        public ScrollRect scrollRect;
        public RayCastHelper rayCastHelper;

        public float scrollSensitivity = 100f;

        public float mouseScrollScalar = 10f;

        public float joystickScrollScalar = 10f;

        private float currentRightVerticalValue = 0;

        void Start()
        {
            
        }

        void Update()
        {
            float mouseScrollDelta = Input.GetAxis("Mouse ScrollWheel");

            if (mouseScrollDelta != 0)
            {
                if (rayCastHelper.RayTestFromViewPoint())
                {
                    Scroll(mouseScrollDelta * mouseScrollScalar);

                    Debug.Log($"Mouse Scroll Delta: {mouseScrollDelta}");
                }
            }

            if (currentRightVerticalValue != 0)
            {
                if (rayCastHelper.RayTestFromRightHand())
                {
                    Scroll(currentRightVerticalValue * joystickScrollScalar * Time.deltaTime);
                    Debug.Log($"Right Hand Scroll Value: {currentRightVerticalValue}");
                }
            }
        }

        private void Scroll(float value)
        {
            float contentHeight = scrollRect.content.rect.height;
            float currentAbsPosition = scrollRect.verticalNormalizedPosition * contentHeight;
            float newAbsPosition = currentAbsPosition + (value * scrollSensitivity);
            float normalizedPosition = Mathf.Clamp01(newAbsPosition / contentHeight);
            scrollRect.verticalNormalizedPosition = normalizedPosition;

            Debug.Log($"Scroll Value: {value}, Current Position: {currentAbsPosition}, New Position: {newAbsPosition}, Normalized Position: {normalizedPosition}");
        }

        public override void InputLookVertical(float value, VRC.Udon.Common.UdonInputEventArgs args)
        {
            if (InputManager.IsUsingHandController())
            {
                if (args.handType == VRC.Udon.Common.HandType.RIGHT)
                    currentRightVerticalValue = value;
            }
            else
                currentRightVerticalValue = 0;
        }
    }
}