
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace RayCastUtils {

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RayCastHelper : UdonSharpBehaviour
    {
        public Collider targetCollider;

        public LayerMask layerMask;

        public float maxDistance = 10f;

        public QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Collide;

        void Start()
        {

        }

        public bool RayTest(Vector3 origin, Vector3 direction, float distance)
        {
            RaycastHit hit;
            if (!Physics.Raycast(origin, direction, out hit, distance, layerMask.value, queryTriggerInteraction))
                return false;

            if (hit.collider != targetCollider)
                return false;

            Debug.Log($"[RayCastHelper] Raycast hit {hit.collider.name} at {hit.point}");
            return true;
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