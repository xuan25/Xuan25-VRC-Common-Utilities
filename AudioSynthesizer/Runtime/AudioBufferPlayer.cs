
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.AudioSynthesizer
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]

    public class AudioBufferPlayer : AudioFilterBase
    {
        [HideInInspector] public float[] audioBuffer;

        [HideInInspector] public int playingSample = 0;

        [HideInInspector] public bool playing = false;

        [HideInInspector] public int numChannels = 0;

        internal override void OnAudioFilterRead(float[] data, int channels)
        {
            if (!playing || audioBuffer.Length == 0)
            {
                numChannels = 0;
                return;
            }

            numChannels = channels;

            int numSamplesBlock = data.Length / channels;
            int numSamplesBufferMax = audioBuffer.Length / channels;

            for (int sample = 0; sample < numSamplesBlock; sample++)
            {
                for (int channel = 0; channel < channels; channel++)
                {
                    float sampleValue = audioBuffer[playingSample + channel];
                    data[sample * channels + channel] = sampleValue;
                }
                playingSample = (playingSample + 1) % numSamplesBufferMax;
            }
        }
    }
}