
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AudioStateInteractable : UdonSharpBehaviour
{
    public AudioStateController audioStateController;
    public bool muteSet = false;
    public bool muteUnset = false;
    public bool surroundingSet = false;
    public bool surroundingUnset = false;

    void Start()
    {
        
    }

    public override void Interact()
    {
        Set();
    }

    private void Set()
    {
        if (muteSet)
        {
            audioStateController.MuteSet();
        }
        else if (muteUnset)
        {
            audioStateController.MuteReset();
        }

        if (surroundingSet)
        {
            audioStateController.SurroundingSet();
        }
        else if (surroundingUnset)
        {
            audioStateController.SurroundingReset();
        }
    }
}
