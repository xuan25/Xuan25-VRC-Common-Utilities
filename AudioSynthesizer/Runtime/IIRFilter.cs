
using System;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Cmp;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Xuan25.AudioSynthesizer;

public class IIRFilter : AudioFilterBase
{
    // b0 is the first pole coefficient, which is the scaling factor for the input
    [SerializeField] public float b0 = 1.0f;

    // b is the array of pole coefficients
    // Note: b[0] is typically the scaling factor (b0), so it is not included in the array.
    [SerializeField] public float[] b;

    // a is the array of zero coefficients
    // Note: a[0] is typically 1.0 for normalized filters, so it is not included in the array.
    [SerializeField] public float[] a;

    [HideInInspector] public int validOutputs = -1;

    // Previous inputs of the filter
    // [0,channel] is the most recent inputs
    float[] xs;
    
    // Previous outputs of the filter
    // [0,channel] is the most recent outputs
    float[] ys;

    internal override void OnAudioFilterRead(float[] data, int channels)
    {
        if (xs == null || xs.Length != b.Length * channels)
        {
            xs = new float[b.Length * channels];
        }

        if (ys == null || ys.Length != a.Length * channels)
        {
            ys = new float[a.Length * channels];
        }

        int numSamples = data.Length / channels;

        for (int sampleIdx = 0; sampleIdx < numSamples; sampleIdx++)
        {
            for (int channel = 0; channel < channels; channel++)
            {
                float input = data[sampleIdx * channels + channel];
                float output = b0 * input;

                // Apply pole coefficients
                for (int j = 0; j < b.Length; j++)
                {
                    output += b[j] * xs[j * channels + channel];
                }

                // Apply zero coefficients
                for (int j = 0; j < a.Length; j++)
                {
                    output -= a[j] * ys[j * channels + channel];
                }

                // shift previous inputs and outputs
                for (int j = 1; j < b.Length; j++)
                {
                    xs[(b.Length - j) * channels + channel] = xs[(b.Length - j - 1) * channels + channel];
                }

                for (int j = 1; j < a.Length; j++)
                {
                    ys[(a.Length - j) * channels + channel] = ys[(a.Length - j - 1) * channels + channel];
                }

                if (float.IsNaN(output) || float.IsInfinity(output))
                {
                    output = 0.0f; // Clamp to zero if the output is invalid
                    validOutputs = 0; // Reset valid outputs count
                }
                else
                {
                    validOutputs++; // Increment valid outputs count
                }

                // Store the current input and output for the next iteration
                if (b.Length > 0)
                    xs[channel] = input;
                if (a.Length > 0)
                    ys[channel] = output;

                data[sampleIdx * channels + channel] = Mathf.Clamp(output, -1.0f, 1.0f);
            }
        }
    }
}
