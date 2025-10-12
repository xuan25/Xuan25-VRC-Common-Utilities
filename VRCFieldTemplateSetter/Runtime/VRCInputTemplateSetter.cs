
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VRCFieldTemplateSetter
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class VRCInputTemplateSetter : UdonSharpBehaviour
    {
        public TMPro.TMP_InputField vRCInputField;
        public string inputTemplate;

        // private bool isFocusedLast = false;

        void Start()
        {
            if (vRCInputField == null)
            {
                vRCInputField = GetComponent<TMPro.TMP_InputField>();
            }
            vRCInputField.text = inputTemplate;
            // isFocusedLast = vRCInputField.isFocused;
        }

        void Update()
        {
            // if (vRCInputField.isFocused != isFocusedLast)
            // {
            //     Debug.Log("[VRCInputTemplateSetter] Focus changed");
            //     isFocusedLast = vRCInputField.isFocused;
            //     Debug.Log("[VRCInputTemplateSetter] Focus changed to " + isFocusedLast);

            //     if (vRCInputField.isFocused && vRCInputField.text == string.Empty)
            //     {
            //         Debug.Log("[VRCInputTemplateSetter] Setting text to template");
            //         vRCInputField.text = inputTemplate;
            //     }
            // }
        }

        public void VRCInputTemplateSetter_OnEndEdit()
        {
            // FIXME: OnEndEdit wont trigger if the input field closed by clicking [x] button
            Debug.Log("[VRCInputTemplateSetter] OnEndEdit");
            // if (vRCInputField.text == inputTemplate)
            // {
            //     Debug.Log("[VRCInputTemplateSetter]  Text is same as template, clearing");
            //     vRCInputField.text = string.Empty;
            // }
        }

        public void VRCInputTemplateSetter_OnValueChanged()
        {
            Debug.Log("[VRCInputTemplateSetter] OnValueUpdated");
            if (vRCInputField.text != inputTemplate)
            {
                Debug.Log("[VRCInputTemplateSetter] Setting text to template");
                vRCInputField.text = inputTemplate;
            }
        }
    }
}