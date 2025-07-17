
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

#if YAMASTREAM
[RequireComponent(typeof(AudioSource))]
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class BGMVolumeControlYamaPlayer : Yamadev.YamaStream.Listener
{
    public Yamadev.YamaStream.Controller controller;

    private AudioSource audioSource;

    [Range(0, 1)] public float volume = 1;

    [SerializeField, Range(0, 10)] float fadeTime = 1;

    private bool isVideoPlaying = false;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        controller.AddListener(this);
    }

    void OnEnable() {
        isVideoPlaying = controller.enabled && controller.gameObject.activeSelf && controller.IsPlaying && !controller.SlideMode;
    }

    void Update()
    {
        if (audioSource == null)
        {
            return;
        }
        float targetVolume = isVideoPlaying ? 0 : volume;
        audioSource.volume = fadeTime > 0 ? Mathf.MoveTowards(audioSource.volume, targetVolume, Time.deltaTime / fadeTime) : targetVolume;
    }

    public override void OnVideoStart() => isVideoPlaying = !controller.SlideMode;

    public override void OnVideoPlay() => isVideoPlaying = !controller.SlideMode;

    public override void OnVideoPause() => isVideoPlaying = false;

    public override void OnVideoEnd() => isVideoPlaying = false;
}
#else
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class BGMVolumeControlYamaPlayer : UdonSharpBehaviour
{
}
#endif