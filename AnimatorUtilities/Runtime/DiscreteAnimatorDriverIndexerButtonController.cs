using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace AnimatorUtilities
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class DiscreteAnimatorDriverIndexerButtonController : DiscreteAnimatorDriverIndexer
    {
        [SerializeField] public Button[] buttons;

        public void OnButtonClicked()
        {
            SetParameter();
        }
    }

}