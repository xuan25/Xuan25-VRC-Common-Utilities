
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Playlist {

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TextFieldSetter : UdonSharpBehaviour
    {
        public TMP_InputField textField;

        public string text;

        void Start()
        {

        }

        public override void Interact()
        {
            Debug.Log("[Playlist] Setting text to " + text);
            textField.text = text;
        }
    }

}