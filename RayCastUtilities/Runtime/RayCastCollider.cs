
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace RayCastUtils {

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RayCastCollider : UdonSharpBehaviour
    {
        public Collider targetCollider;

        public float maxDistance = 10f;

        public QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Collide;

        void Start()
        {
            if (targetCollider == null)
            {
                targetCollider = GetComponent<Collider>();
            }
        }

        public bool RayTest(Vector3 origin, Vector3 direction, float maxDistance)
        {
            return targetCollider.Raycast(new Ray(origin, direction), out RaycastHit hitInfo, maxDistance);
        }

        public bool RayTestFromTracking(VRCPlayerApi.TrackingDataType trackingDataType, Vector3 rayVector)
        {
            if (targetCollider == null)
                return false;

            VRCPlayerApi localPlayer = Networking.LocalPlayer;

            Vector3 origin = localPlayer.GetTrackingData(trackingDataType).position;
            Vector3 direction = localPlayer.GetTrackingData(trackingDataType).rotation * rayVector;

            return RayTest(origin, direction, maxDistance);
        }

        public bool RayTestFromViewPoint()
        {
            return RayTestFromTracking(VRCPlayerApi.TrackingDataType.Head, Vector3.forward);
        }

        public bool RayTestFromLeftHand()
        {
            // math.sin(40.0 / 180 * math.pi) / math.cos(40.0 / 180 * math.pi)
            return RayTestFromTracking(VRCPlayerApi.TrackingDataType.LeftHand, Vector3.Normalize(0.8390996311772799f * Vector3.left + Vector3.forward));
        }

        public bool RayTestFromRightHand()
        {
            return RayTestFromTracking(VRCPlayerApi.TrackingDataType.RightHand, Vector3.Normalize(0.8390996311772799f * Vector3.right + Vector3.forward));
        }
    }

}