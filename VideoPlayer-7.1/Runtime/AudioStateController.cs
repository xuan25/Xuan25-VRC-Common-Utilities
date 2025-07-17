
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AudioStateController : UdonSharpBehaviour
{
    public AudioSource defaultSourceBuiltIn;
    public AudioSource defaultSourceAVPro;
    public AudioSource[] surroundingSources;
    
    public bool isSurrounding = true;

    public bool isMuted = false;

    private bool surroundingDefault = true;

    private bool muteDefault = false;

    void Start()
    {
        surroundingDefault = isSurrounding;
        muteDefault = isMuted;

        UpdateAudioSources();
    }

    public void Reset()
    {
        SetSurrounding(surroundingDefault);
        SetMute(muteDefault);
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
        Debug.Log($"[AudioStateController] UpdateAudioSources: isSurrounding={isSurrounding}, isMuted={isMuted}");
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
