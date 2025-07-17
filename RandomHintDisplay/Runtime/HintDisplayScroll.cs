
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace RandomHintDisplay
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class HintDisplayScroll : HintDisplay
    {
        public GameObject hintContainer;
        public GameObject hintPrefab;

        public Animator hintAnimator;

        private bool isHintVisible = false;

        public int numberOfHintsToScroll = 30;

        private HintDisplayScrollItem[] hintItemsScroll;

        private HintDisplayScrollItem hintItemFinal;

        [UdonSynced]
        private string[] hintsScroll = new string[0];

        [UdonSynced]
        private string finalHint = string.Empty;

        void Start()
        {
            // clear any existing hint items if they exist
            foreach (Transform child in hintContainer.transform)
            {
                Destroy(child.gameObject);
            }
            // initialize hint items
            hintItemsScroll = new HintDisplayScrollItem[numberOfHintsToScroll];
            for (int i = 0; i < numberOfHintsToScroll; i++)
            {
                GameObject hintItemObject = Instantiate(hintPrefab, hintContainer.transform);
                HintDisplayScrollItem hintItem = hintItemObject.GetComponent<HintDisplayScrollItem>();
                hintItem.gameObject.SetActive(false);
                hintItemsScroll[i] = hintItem;
            }

            GameObject hintItemFinalObject = Instantiate(hintPrefab, hintContainer.transform);
            hintItemFinal = hintItemFinalObject.GetComponent<HintDisplayScrollItem>();
            hintItemFinal.gameObject.SetActive(true);
        }

        
        public override void ShowHint(HintFeeder hintFeeder, bool sync)
        {
            hintsScroll = hintFeeder.SampleHintsWithReplacement(numberOfHintsToScroll);
            finalHint = hintFeeder.SampleHintsWithoutReplacementSync(1)[0];

            if (sync)
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                RequestSerialization();
            }

            UpdateDisplay();
        }

        public void UpdateDisplay(){
            for (int i = 0; i < numberOfHintsToScroll; i++)
            {
                if (i < hintsScroll.Length)
                {
                    hintItemsScroll[i].SetHint(hintsScroll[i]);
                    hintItemsScroll[i].gameObject.SetActive(true);
                }
                else
                {
                    hintItemsScroll[i].gameObject.SetActive(false);
                }
            }

            hintItemFinal.SetHint(finalHint);

            hintAnimator.ResetTrigger("Scroll");
            hintAnimator.SetTrigger("Scroll");
            SetHintVisibility(true);
        }

        public override void OnDeserialization()
        {
            if (hintsScroll.Length > 0)
            {
                UpdateDisplay();
            }
        }

        private void SetHintVisibility(bool isVisible)
        {
            isHintVisible = isVisible;
            if (isHintVisible)
            {
                hintAnimator.ResetTrigger("Hide");
                hintAnimator.SetTrigger("Show");
            }
            else
            {
                hintAnimator.ResetTrigger("Show");
                hintAnimator.SetTrigger("Hide");
            }
        }

        public override void Interact()
        {
            SetHintVisibility(!isHintVisible);
        }
    }
}
