
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using static VRC.SDKBase.VRCPlayerApi;

namespace Playlist {

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FloatUIController : UdonSharpBehaviour
    {
        public float floatDistance = 0.5f;
        public float floatScale = 0.5f;
        // public float triggerDelay = 0.5f;

        public Transform holdExactAnchor;
        public Transform contentAnchor;

        public GameObject resetToDockInteract;

        public float holdForwardSpeed = 1f;

        private VRCPickup holdPickup;

        private Vector3 dockingPosition;
        private Vector3 dockingRotation;
        private Vector3 dockingScale;

        private bool isFloating = false;

        // private float triggerDuration = 0f;
        private float currentRightVerticalValue = 0;
        // private bool currentLeftUseValue = false;
        // private bool currentRightUseValue = false;

        private Vector3 contentLocalPositionDefault;

        public bool IsHeld => holdPickup != null && holdPickup.IsHeld;

        void Start()
        {
            dockingPosition = transform.position;
            dockingRotation = transform.rotation.eulerAngles;
            dockingScale = transform.localScale;

            holdPickup = gameObject.GetComponent<VRCPickup>();
            if (holdPickup != null)
                holdPickup.pickupable = false;

            InitHold();
        }

        void Update()
        {
            // float newTriggerDuration;
            // if (!holdPickup.IsHeld)
            // {
            //     if (Input.GetKey(KeyCode.E) || (currentLeftUseValue && currentRightUseValue))
            //         newTriggerDuration = triggerDuration + Time.deltaTime;
            //     else
            //         newTriggerDuration = 0f;
            // }
            if (holdPickup.IsHeld)
            {
                // newTriggerDuration = 0f;

                // allow distance to be adjusted while holding with right thumb stick or keyboard
                if (currentRightVerticalValue > 0.1f || currentRightVerticalValue < -0.1f)
                    ForwardHold(currentRightVerticalValue * holdForwardSpeed);
                if (Input.GetKey(KeyCode.E))
                    ForwardHold(1f * holdForwardSpeed);
                if (Input.GetKey(KeyCode.Q))
                    ForwardHold(-1f * holdForwardSpeed);
            }

            // if (newTriggerDuration > triggerDelay && triggerDuration < triggerDelay)
            //     OnToggle();

            // triggerDuration = newTriggerDuration;
        }

        private void InitHold()
        {
            holdExactAnchor.localPosition = new Vector3(0, 0, -floatDistance);

            contentAnchor = transform.GetChild(0);
            contentLocalPositionDefault = contentAnchor.localPosition;
        }

        private void ForwardHold(float speed)
        {
            Vector3 newChildPosition = contentAnchor.localPosition;
            newChildPosition.z += speed * Time.deltaTime;
            contentAnchor.localPosition = newChildPosition;
        }

        private void ResetHold()
        {
            holdExactAnchor.localPosition = new Vector3(0, 0, -floatDistance);
            contentAnchor.localPosition = contentLocalPositionDefault;
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

        // public override void InputUse(bool value, VRC.Udon.Common.UdonInputEventArgs args) {
        //     if (InputManager.IsUsingHandController())
        //     {
        //         if (args.handType == VRC.Udon.Common.HandType.LEFT)
        //             currentLeftUseValue = value;
        //         else if (args.handType == VRC.Udon.Common.HandType.RIGHT)
        //             currentRightUseValue = value;
        //     }
        //     else
        //     {
        //         currentLeftUseValue = false;
        //         currentRightUseValue = false;
        //     }
        // }

        public override void OnPickup()	
        {
            isFloating = true;
            resetToDockInteract.SetActive(true);
        }

        public override void OnDrop()
        {
            float offset = contentAnchor.localPosition.z - contentLocalPositionDefault.z;

            // transfer offsets from childTransform to holdExactAnchor and reset childTransform
            Vector3 newHoldPosition = holdExactAnchor.localPosition;
            newHoldPosition.z -= offset;
            holdExactAnchor.localPosition = newHoldPosition;
            contentAnchor.localPosition = contentLocalPositionDefault;

            // translate gameObject with regards to GameObject's orientation and scale
            Vector3 gameObjectOffset = gameObject.transform.rotation * new Vector3(0, 0, offset * gameObject.transform.localScale.z);
            gameObject.transform.position += gameObjectOffset;
        }

        public void OnToggle()
        {
            if (isFloating)
            {
                OnReset();
            }
            else
            {
                OnFloat();
            }
        }

        public void OnFloat()
        {
            Debug.Log("[FloatUIController] Setting panel to floating state.");
            isFloating = true;
            TrackingData trackingData = Networking.LocalPlayer.GetTrackingData(TrackingDataType.Head);
            Vector3 headPosition = trackingData.position;
            Vector3 headRotation = trackingData.rotation.eulerAngles;

            Vector3 newPosition = headPosition + Quaternion.Euler(headRotation) * Vector3.forward * floatDistance;
            transform.position = newPosition;
            transform.rotation = Quaternion.Euler(headRotation);

            Vector3 newScale = dockingScale * floatScale;
            transform.localScale = newScale;

            ForceMeshUpdate();
            
            if (holdPickup != null)
                holdPickup.pickupable = true;
            if (resetToDockInteract != null)
                resetToDockInteract.SetActive(true);
        }

        public void OnReset()
        {
            Debug.Log("[FloatUIController] Resetting panel to docking state.");
            isFloating = false;
            transform.position = dockingPosition;
            transform.rotation = Quaternion.Euler(dockingRotation);
            transform.localScale = dockingScale;

            ForceMeshUpdate();

            if (holdPickup != null)
                holdPickup.pickupable = false;
            if (resetToDockInteract != null)
                resetToDockInteract.SetActive(false);

            ResetHold();
        }

        private void ForceMeshUpdate()
        {
            Debug.Log("[FloatUIController] Forcing mesh update for all text objects.");
            int count = 0;
            // Force mesh update for all child under this object
            foreach(TextMeshProUGUI textMeshProUGUI in GetComponentsInChildren<TextMeshProUGUI>())
            {
                textMeshProUGUI.ForceMeshUpdate();
                count++;
            }

            Debug.Log($"[FloatUIController] Force mesh update for {count} text objects.");
        }
    }

}