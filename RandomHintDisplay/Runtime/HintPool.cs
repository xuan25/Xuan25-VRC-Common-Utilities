
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace RandomHintDisplay
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class HintPool : UdonSharpBehaviour
    {
// #if UNITY_EDITOR
//         [SerializeField]
//         public string editorSettingHintText;
// #endif
        public string[] hints = new string[0];
        
        void Start()
        {
            
        }
    }
}
