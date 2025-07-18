
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
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

        public bool horizontal = false;

        private RectTransform scrollRectTransform;

        private float currentRightVerticalValue = 0;

        void Start()
        {
            scrollRectTransform = scrollRect.GetComponent<RectTransform>();
        }

        public void FixedUpdate()
        {
            // Adjust the size of the collider to match the scroll view
            
            if (rayCastHelper.targetCollider.GetType() == typeof(BoxCollider))
            {
                BoxCollider boxCollider = (BoxCollider)rayCastHelper.targetCollider;
                boxCollider.size = scrollRectTransform.rect.size;
                boxCollider.center = scrollRectTransform.rect.center;
            }

            // Check for mouse scroll input and right hand joystick input

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
            if (!horizontal)
            {
                float contentHeight = scrollRect.content.rect.height;
                float currentAbsPosition = scrollRect.verticalNormalizedPosition * contentHeight;
                float newAbsPosition = currentAbsPosition + (value * scrollSensitivity);
                float normalizedPosition = Mathf.Clamp01(newAbsPosition / contentHeight);
                scrollRect.verticalNormalizedPosition = normalizedPosition;

                Debug.Log($"Vertical Scroll Value: {value}, Current Position: {currentAbsPosition}, New Position: {newAbsPosition}, Normalized Position: {normalizedPosition}");
            }
            else
            {
                float contentWidth = scrollRect.content.rect.width;
                float currentAbsPosition = scrollRect.horizontalNormalizedPosition * contentWidth;
                float newAbsPosition = currentAbsPosition + (-value * scrollSensitivity);
                float normalizedPosition = Mathf.Clamp01(newAbsPosition / contentWidth);
                scrollRect.horizontalNormalizedPosition = normalizedPosition;

                Debug.Log($"Horizontal Scroll Value: {value}, Current Position: {currentAbsPosition}, New Position: {newAbsPosition}, Normalized Position: {normalizedPosition}");
            }
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