
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AudioStateTriggerZone : UdonSharpBehaviour
{
    public AudioStateController audioStateController;
    public bool muteSet = false;
    public bool muteUnset = false;
    public bool surroundingSet = false;
    public bool surroundingUnset = false;

    void Start()
    {
        
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            Set();
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            Reset();
        }
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

    private void Reset()
    {
        audioStateController.Reset();
    }
}
