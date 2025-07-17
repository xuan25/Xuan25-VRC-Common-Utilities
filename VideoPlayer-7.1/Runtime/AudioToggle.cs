
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class AudioToggle : UdonSharpBehaviour
{
    public AudioSource defaultSourceBuiltIn;
    public AudioSource defaultSourceAVPro;
    public AudioSource[] surroundingSources;
    
    public bool isSurrounding = true;

    public bool isMuted = false;

    void Start()
    {
        UpdateAudioSources();
    }

    public override void Interact()
    {
        ToggleSurrounding();
    }

    public void ToggleSurrounding()
    {
        SetSurrounding(!isSurrounding);
    }

    public void SurroundingSet()
    {
        SetSurrounding(true);
    }

    public void SurroundingReset()
    {
        SetSurrounding(false);
    }

    public void SetSurrounding(bool value)
    {
        isSurrounding = value;
        UpdateAudioSources();
    }

    public void ToggleMute()
    {
        SetMute(!isMuted);
    }

    public void MuteSet()
    {
        SetMute(true);
    }

    public void MuteReset()
    {
        SetMute(false);
    }

    public void SetMute(bool value)
    {
        isMuted = value;
        UpdateAudioSources();
    }

    private void UpdateAudioSources()
    {
        defaultSourceBuiltIn.mute = isMuted;

        if (isMuted)
        {
            defaultSourceAVPro.mute = true;
            foreach (AudioSource source in surroundingSources)
            {
                source.mute = true;
            }
            return;
        }
        
        defaultSourceAVPro.mute = isSurrounding;
        foreach (AudioSource source in surroundingSources)
        {
            source.mute = !isSurrounding;
        }
    }
}
