
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class VRCUrlTemplateSetter : UdonSharpBehaviour
{
    public VRCUrlInputField vRCUrlInputField;
    public VRCUrl urlTemplate;

    private bool isFocusedLast = false;

    void Start()
    {
        if (vRCUrlInputField == null)
        {
            vRCUrlInputField = GetComponent<VRCUrlInputField>();
        }
        isFocusedLast = vRCUrlInputField.isFocused;
    }

    void Update()
    {
        if (vRCUrlInputField.isFocused != isFocusedLast)
        {
            Debug.Log("[VRCUrlTemplateSetter] Focus changed");
            isFocusedLast = vRCUrlInputField.isFocused;
            Debug.Log("[VRCUrlTemplateSetter] Focus changed to " + isFocusedLast);

            if (vRCUrlInputField.isFocused && vRCUrlInputField.GetUrl().Get() == string.Empty)
            {
                Debug.Log("[VRCUrlTemplateSetter] Setting URL to template");
                vRCUrlInputField.SetUrl(urlTemplate);
            }
        }
    }
    
    public void VRCUrlTemplateSetter_OnEndEdit()
    {
        // FIXME: OnEndEdit wont trigger if the input field closed by clicking [x] button
        Debug.Log("[VRCUrlTemplateSetter] OnEndEdit");
        if (vRCUrlInputField.GetUrl().Get() == urlTemplate.Get()){
            Debug.Log("[VRCUrlTemplateSetter]  URL is same as template, clearing");
            vRCUrlInputField.SetUrl(VRCUrl.Empty);
        }
    }

    public void VRCUrlTemplateSetter_OnValueChanged()
    {
        Debug.Log("[VRCUrlTemplateSetter] OnValueUpdated");
    }
}
