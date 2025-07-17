using UnityEngine;
using UdonSharp;

namespace Xuan25.AudioSynthesizer
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Oscillator : AudioFilterBase
    {
        [SerializeField, Range(0, 1)] public float amplitude = 0.3f;
        [SerializeField] public float frequency = 261.62f;
        [SerializeField, Range(0, 1)] public float skewness = 0.0f;

        private float sampleRate = 48000;

        private float currentPhase = 0.0f;
        private float lastFrequency = 0.0f;
        private float cachedPhaseIncrement = 0.0f;

        void Start()
        {
            sampleRate = AudioSettings.outputSampleRate;
        }

        internal override void OnAudioFilterRead(float[] data, int channels)
        {
            // Debug.Log($"OnAudioFilterRead called with {data.Length} samples and {channels} channels.");
            float frequency = this.frequency;
            float amplitude = this.amplitude;
            float skewness = this.skewness;

            if (lastFrequency != frequency)
            {
                lastFrequency = frequency;
                cachedPhaseIncrement = (float)(2.0f * Mathf.PI * frequency / sampleRate);
            }

            float phaseIncrement = cachedPhaseIncrement;

            for (int i = 0; i < data.Length; i += channels)
            {
                // Use continuous phase to avoid discontinuities
                float value = 0.0f;
                if (skewness == 0.0f)
                {
                    // No skewness, use normal sine wave
                    // \sin\left(x\right)
                    value = Mathf.Sin(currentPhase);
                }
                // else if (skewness == 1.0f)
                // {
                //     // Full skewness, use sawtooth wave
                //     // -\left(2\cdot\operatorname{mod}\left(x/(2.0*\pi),1\right)-1\right)
                //     value = -(2.0f * Mathf.Repeat((currentPhase) / (2.0f * Mathf.PI), 1.0f) - 1.0f);
                // }
                else
                {
                    // Apply skewness to the phase increment
                    // \frac{1}{t}\arctan\left(\frac{t\sin\left(x\right)}{1-t\ \cos\left(x\right)}\right)
                    float t = skewness;
                    value = 1 / t * Mathf.Atan(t * Mathf.Sin(currentPhase) / (1 - t * Mathf.Cos(currentPhase)));
                }

                value *= amplitude;

                for (int j = 0; j < channels; j++)
                {
                    data[i + j] += value;
                }

                // Increment phase for next sample
                currentPhase += phaseIncrement;
            }

            // Keep phase in reasonable range to prevent floating point precision issues
            currentPhase = Mathf.Repeat(currentPhase, 2.0f * Mathf.PI);
        }
    }
}