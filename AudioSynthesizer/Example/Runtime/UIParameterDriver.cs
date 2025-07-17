
using UdonSharp;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.AudioSynthesizer
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UIParameterDriver : UdonSharpBehaviour
    {
        [SerializeField] private Oscillator oscillator;
        [SerializeField] private Slider AmplitudeSlider;
        [SerializeField] private Slider FrequencySlider;
        [SerializeField] private Slider SkewnessSlider;

        [SerializeField] private IIRFilter iirFilter;
        [SerializeField] private Slider b0Slider;
        [SerializeField] private Slider b1Slider;
        [SerializeField] private Slider b2Slider;
        [SerializeField] private Slider a1Slider;
        [SerializeField] private Slider a2Slider;

        [SerializeField] private GameObject iirErrorIndicator;

        [SerializeField] private AudioBufferFiller audioBufferFiller;

        [SerializeField] private GameObject audioBufferFillerErrorIndicator;

        void Start()
        {
            UpdateAmplitude();
            UpdateFrequency();
            UpdateSkewness();

            RepopulateIIRFilterArray();

            UpdateB0();
            UpdateB1();
            UpdateB2();
            UpdateA1();
            UpdateA2();
        }

        void Update()
        {
            if (iirFilter != null && iirErrorIndicator != null)
            {
                // Check if the IIRFilter is in error state
                if (iirFilter.validOutputs >= 0 && iirFilter.validOutputs < 100)
                {
                    iirErrorIndicator.SetActive(true);
                }
                else
                {
                    iirErrorIndicator.SetActive(false);
                }
            }
            if (audioBufferFiller != null && audioBufferFillerErrorIndicator != null)
            {
                // Check if the AudioBufferFiller is in error state
                if (audioBufferFiller.keepUpCount >= 0 && audioBufferFiller.keepUpCount < 3)
                {
                    audioBufferFillerErrorIndicator.SetActive(true);
                }
                else
                {
                    audioBufferFillerErrorIndicator.SetActive(false);
                }
            }
        }

        public void RepopulateIIRFilterArray()
        {
            if (iirFilter != null)
            {
                if (b2Slider != null)
                {
                    iirFilter.b = new float[2];
                    Debug.Log($"Repopulated IIRFilter b array with 2 elements.");
                }
                else if (b1Slider != null)
                {
                    iirFilter.b = new float[1];
                    Debug.Log($"Repopulated IIRFilter b array with 1 element.");
                }
                else
                {
                    iirFilter.b = new float[0];
                    Debug.Log($"Repopulated IIRFilter b array with 0 elements.");
                }
                if (a2Slider != null)
                {
                    iirFilter.a = new float[2];
                    Debug.Log($"Repopulated IIRFilter a array with 2 elements.");
                }
                else if (a1Slider != null)
                {
                    iirFilter.a = new float[1];
                    Debug.Log($"Repopulated IIRFilter a array with 1 element.");
                }
                else
                {
                    iirFilter.a = new float[0];
                    Debug.Log($"Repopulated IIRFilter a array with 0 elements.");
                }
            }

            UpdateB0();
            UpdateB1();
            UpdateB2();
            UpdateA1();
            UpdateA2();
        }

        public void UpdateAmplitude()
        {
            if (oscillator != null)
            {
                oscillator.amplitude = AmplitudeSlider.value;
            }
        }

        public void UpdateFrequency()
        {
            if (oscillator != null)
            {
                oscillator.frequency = FrequencySlider.value;
            }
        }

        public void UpdateSkewness()
        {
            if (oscillator != null)
            {
                oscillator.skewness = SkewnessSlider.value;
            }
        }

        public void UpdateB0()
        {
            if (iirFilter != null)
            {
                iirFilter.b0 = b0Slider.value;
            }
        }

        public void UpdateB1()
        {
            if (iirFilter != null && iirFilter.b.Length > 0)
            {
                iirFilter.b[0] = b1Slider.value;
            }
        }

        public void UpdateB2()
        {
            if (iirFilter != null && iirFilter.b.Length > 1)
            {
                iirFilter.b[1] = b2Slider.value;
            }
        }

        public void UpdateA1()
        {
            if (iirFilter != null && iirFilter.a.Length > 0)
            {
                iirFilter.a[0] = a1Slider.value;
            }
        }

        public void UpdateA2()
        {
            if (iirFilter != null && iirFilter.a.Length > 1)
            {
                iirFilter.a[1] = a2Slider.value;
            }
        }
    }
}