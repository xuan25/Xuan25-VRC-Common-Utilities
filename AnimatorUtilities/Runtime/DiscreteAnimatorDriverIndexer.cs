
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace AnimatorUtilities
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class DiscreteAnimatorDriverIndexer : UdonSharpBehaviour
    {
        [SerializeField] private DiscreteAnimatorDriver target;

        [SerializeField] public int index = 0;

        public void SetParameter()
        {
            if (target != null)
            {
                target.SetParameter(index);
            }
        }

        public override void Interact()
        {
            SetParameter();
        }
    }
}