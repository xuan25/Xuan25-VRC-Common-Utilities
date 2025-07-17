
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace RandomHintDisplay
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class HintDisplayScrollItem : UdonSharpBehaviour
    {

        public TextMeshProUGUI hintText;

        void Start()
        {
            
        }

        public void SetHint(string hint)
        {
            hintText.text = hint;
        }
    }
}
