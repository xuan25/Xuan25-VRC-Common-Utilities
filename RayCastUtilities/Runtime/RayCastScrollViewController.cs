
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.RayCastUtilities {

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RayCastScrollViewController : RayCastControllerBase
    {
        public ScrollRect scrollRect;
        private RectTransform scrollRectTransform;
        public float scrollSensitivity = 100f;
        public bool horizontal = false;

        public void Start()
        {
            scrollRectTransform = scrollRect.GetComponent<RectTransform>();
        }

        public override void Update()
        {
            // Adjust the size of the collider to match the scroll view

            if (rayCastCollider.targetCollider.GetType() == typeof(BoxCollider))
            {
                BoxCollider boxCollider = (BoxCollider)rayCastCollider.targetCollider;
                boxCollider.size = scrollRectTransform.rect.size;
                boxCollider.center = scrollRectTransform.rect.center;
            }

            base.Update();
        }

        public override void OnDelta(float value)
        {
            if (!horizontal)
            {
                float contentHeight = scrollRect.content.rect.height;
                float currentAbsPosition = scrollRect.verticalNormalizedPosition * contentHeight;
                float newAbsPosition = currentAbsPosition + (value * scrollSensitivity);
                float normalizedPosition = Mathf.Clamp01(newAbsPosition / contentHeight);
                scrollRect.verticalNormalizedPosition = normalizedPosition;
#if DEBUG
                Debug.Log($"Vertical Scroll Value: {value}, Current Position: {currentAbsPosition}, New Position: {newAbsPosition}, Normalized Position: {normalizedPosition}");
#endif
            }
            else
            {
                float contentWidth = scrollRect.content.rect.width;
                float currentAbsPosition = scrollRect.horizontalNormalizedPosition * contentWidth;
                float newAbsPosition = currentAbsPosition + (-value * scrollSensitivity);
                float normalizedPosition = Mathf.Clamp01(newAbsPosition / contentWidth);
                scrollRect.horizontalNormalizedPosition = normalizedPosition;
#if DEBUG
                Debug.Log($"Horizontal Scroll Value: {value}, Current Position: {currentAbsPosition}, New Position: {newAbsPosition}, Normalized Position: {normalizedPosition}");
#endif
            }
        }
        
        public override void OnReset()
        {
        }
    }
}