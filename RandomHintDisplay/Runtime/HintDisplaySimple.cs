
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace RandomHintDisplay
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class HintDisplaySimple : HintDisplay
    {
        
        public Animator hintAnimator;

        public TextMeshProUGUI hintText;

        private bool isHintVisible = false;

        [UdonSynced]
        private string hint = string.Empty;


        void Start()
        {
            
        }

        public override void ShowHint(HintFeeder hintFeeder, bool sync)
        {
            hint = hintFeeder.SampleHintsWithoutReplacementSync(1)[0];

            if (sync)
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                RequestSerialization();
            }

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            hintText.text = hint;
            SetHintVisibility(true);
        }

        public override void OnDeserialization()
        {
            if (hint != string.Empty)
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