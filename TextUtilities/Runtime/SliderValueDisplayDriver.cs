
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace TextUtilities
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]

    public class SliderValueDisplayDriver : UdonSharpBehaviour
    {
        [SerializeField] public Slider slider;
        [SerializeField] public TextMeshProUGUI valueText;

        void Start()
        {
            UpdateValueText();
        }

        public void OnSliderValueChanged()
        {
            UpdateValueText();
        }

        public void UpdateValueText()
        {
            if (valueText != null && slider != null)
            {
                valueText.text = slider.value.ToString("F2");
            }
        }
    }

}
