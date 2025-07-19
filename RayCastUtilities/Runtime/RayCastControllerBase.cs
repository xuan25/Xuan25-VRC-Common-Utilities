
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


namespace RayCastUtils {

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class RayCastControllerBase : UdonSharpBehaviour
    {
        public RayCastCollider rayCastCollider;

        public float scrollSensitivity = 100f;

        public float mouseScrollScalar = 10f;

        public float joystickScrollScalar = 10f;

        public bool horizontal = false;

        private float currentRightVerticalValue = 0;

        private bool reset = true;

        public virtual void OnDelta(float value)
        {
            Debug.Log($"RayCastControllerBase Delta {value}");
        }
        
        public virtual void OnReset()
        {
            Debug.Log("RayCastControllerBase Reset");
        }

        public virtual void FixedUpdate()
        {
            float mouseScrollDelta = Input.GetAxis("Mouse ScrollWheel");

            if (mouseScrollDelta != 0)
            {
                if (rayCastCollider.RayTestFromViewPoint())
                {
                    OnDelta(mouseScrollDelta * mouseScrollScalar);
#if DEBUG
                    Debug.Log($"Mouse Scroll Delta: {mouseScrollDelta}");
#endif
                }
                reset = false;
            }

            if (currentRightVerticalValue != 0)
            {
                if (rayCastCollider.RayTestFromRightHand())
                {
                    OnDelta(currentRightVerticalValue * joystickScrollScalar * Time.deltaTime);
#if DEBUG
                    Debug.Log($"Right Hand Delta Value: {currentRightVerticalValue}");
#endif
                }
                reset = false;
            }

            if (mouseScrollDelta == 0 && currentRightVerticalValue == 0)
            {
                if (!reset)
                {
                    reset = true;
                    OnReset();
                }
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