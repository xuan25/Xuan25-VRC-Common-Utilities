
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace RandomHintDisplay
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class HintFeeder : UdonSharpBehaviour
    {
        public HintDisplay hintDisplay;

        public HintPool hintPool;

        public bool globalSync = false;

        [UdonSynced]
        private int currentHintIndex = -1;

        [UdonSynced]
        private int[] hintIndices = new int[0];

        void Start()
        {
            currentHintIndex = -1;
            hintIndices = new int[hintPool.hints.Length];
            if (hintIndices.Length > 0)
            {
                for (int i = 0; i < hintIndices.Length; i++)
                {
                    hintIndices[i] = i;
                }
            } 
            else
            {
                Debug.LogWarning("No hints available in the HintFeeder.");
            }
            
        }
        
        public void ShuffleHints()
        {
            if (hintIndices.Length == 0) return;

            float[] randomScores = new float[hintIndices.Length];
            for (int i = 0; i < randomScores.Length; i++)
            {
                randomScores[i] = UnityEngine.Random.Range(0f, 1f);
            }
            Array.Sort((Array)randomScores, hintIndices);
        }

        /// <summary>
        /// Samples hints without replacement.
        /// This method will shuffle the hints and sample them in a round-robin fashion.
        /// When all hints have been sampled, it will shuffle the hints again and start over.
        /// This method is synchronized across all players if globalSync is true.
        /// </summary>
        /// <param name="count">number of hints to sample</param>
        /// <returns>an array of sampled hints. null if no hint is available</returns>
        public string[] SampleHintsWithoutReplacementSync(int count)
        {
            if (hintIndices.Length == 0)
                return null;

            string[] hints = new string[count];

            for (int i = 0; i < count; i++)
            {
                currentHintIndex = (currentHintIndex + 1) % hintIndices.Length;
                if (currentHintIndex == 0)
                {
                    // sample without replacement has been completed
                    // all the samples in the pool has been replaced and shuffled
                    // current index has been reset
                    ShuffleHints();
                }

                hints[i] = hintPool.hints[hintIndices[currentHintIndex]];
            }

            if (globalSync)
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                RequestSerialization();
            }

            return hints;
        }

        /// <summary>
        /// Samples hints with replacement.
        /// This method will randomly select hints from the pool.
        /// It will not shuffle the hints and will allow for the same hint to be selected multiple times.
        /// This method is not synchronized across all players.
        /// </summary>
        /// <param name="count">number of hints to sample</param>
        /// <returns>an array of sampled hints. null if no hint is available</returns>
        public string[] SampleHintsWithReplacement(int count)
        {
            if (hintIndices.Length == 0)
                return null;

            string[] hints = new string[count];

            for (int i = 0; i < count; i++)
            {
                // Randomly select a hint index
                int randomIndex = UnityEngine.Random.Range(0, hintIndices.Length);
                // Fetch the hint using the random index
                hints[i] = hintPool.hints[hintIndices[randomIndex]];
            }

            return hints;
        }

        public void ShowNextHint()
        {
            if (hintIndices.Length == 0)
                return;

            hintDisplay.ShowHint(this, globalSync);
        }

        public override void Interact()
        {
            ShowNextHint();
        }

    }

}


