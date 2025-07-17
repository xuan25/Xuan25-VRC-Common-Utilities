
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.AudioSynthesizer
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AudioBufferFiller : UdonSharpBehaviour
    {
        [SerializeField] private AudioBufferPlayer audioBufferPlayer;

        [SerializeField] private AudioFilterBase[] audioFilters;

        [SerializeField] private int bufferSize = 10240;

        private float[] filterBuffer;

        private int fillingSample = 0;

        [SerializeField] private float maxFillingElapsedTime = 0.01f; // Maximum time to fill the buffer in seconds

        [HideInInspector] public int keepUpCount = -1;

        public void OnEnable()
        {
            audioBufferPlayer.audioBuffer = new float[bufferSize];
            fillingSample = 0;
            audioBufferPlayer.playingSample = 0;
            audioBufferPlayer.playing = true;
        }

        public void OnDisable()
        {
            audioBufferPlayer.playing = false;
            fillingSample = 0;
            audioBufferPlayer.playingSample = 0;
            filterBuffer = null;
        }

        public void FixedUpdate()
        {
            float updateBeginTime = Time.realtimeSinceStartup;
            int numChannels = audioBufferPlayer.numChannels;

            if (filterBuffer == null || filterBuffer.Length != numChannels)
            {
                filterBuffer = new float[numChannels];
            }

            if (numChannels <= 0 || audioBufferPlayer.audioBuffer.Length == 0)
            {
                return; // No channels or empty buffer, nothing to fill
            }

            int numSamplesBufferMax = audioBufferPlayer.audioBuffer.Length / numChannels;
            while (fillingSample != audioBufferPlayer.playingSample)
            {
                // clear the sample buffer
                for (int channel = 0; channel < numChannels; channel++)
                {
                    filterBuffer[channel] = 0;
                }

                // run the audio filters
                foreach (AudioFilterBase filter in audioFilters)
                {
                    filter.OnAudioFilterReadProxy(filterBuffer, numChannels);
                }

                // write the sample buffer to the audio buffer player
                for (int channel = 0; channel < numChannels; channel++)
                {
                    audioBufferPlayer.audioBuffer[fillingSample + channel] = filterBuffer[channel];
                }

                fillingSample = (fillingSample + 1) % numSamplesBufferMax;

                if (Time.realtimeSinceStartup - updateBeginTime > maxFillingElapsedTime)
                {
                    // If we exceed the maximum filling time, break to avoid stalling the main thread
                    keepUpCount = 0;
                    break;
                }
                keepUpCount++;
            }
        }
    }
}