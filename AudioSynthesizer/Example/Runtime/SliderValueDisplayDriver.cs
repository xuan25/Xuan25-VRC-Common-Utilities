
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class SliderValueDisplayDriver : UdonSharpBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI valueText;

    void Start()
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
