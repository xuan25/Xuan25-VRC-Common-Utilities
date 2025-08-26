
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace AnimatorUtilities
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    [RequireComponent(typeof(VRC_Pickup))]
    public class DiscreteAnimatorDriverPickupProxy : UdonSharpBehaviour
    {
        [SerializeField] private DiscreteAnimatorDriver target;

        public override void OnPickupUseDown()
        {
            if (target != null)
            {
                target.SetParameter();
            }
        }
    }
}