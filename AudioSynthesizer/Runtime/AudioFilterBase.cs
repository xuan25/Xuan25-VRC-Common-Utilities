
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.AudioSynthesizer
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class AudioFilterBase : UdonSharpBehaviour
    {
        [HideInInspector]
        private float[] onAudioFilterReadData;

        [HideInInspector]
        private int onAudioFilterReadChannels;

        public void _onAudioFilterRead()
        {
            // Debug.Log($"_onAudioFilterRead called with {onAudioFilterReadData.Length} samples and {onAudioFilterReadChannels} channels.");
            OnAudioFilterRead(onAudioFilterReadData, onAudioFilterReadChannels);
        }

        internal virtual void OnAudioFilterRead(float[] data, int channels)
        {
            // This method should be overridden by subclasses to implement audio processing.
            // The default implementation does nothing.
            Debug.LogWarning($"OnAudioFilterRead not implemented in {nameof(AudioFilterBase)}. Please override this method in derived classes.");
        }

        internal void OnAudioFilterReadProxy(float[] data, int channels)
        {
            OnAudioFilterRead(data, channels);
        }
    }
}
