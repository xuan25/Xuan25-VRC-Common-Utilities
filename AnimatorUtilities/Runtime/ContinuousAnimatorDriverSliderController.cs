
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace AnimatorUtilities
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ContinuousAnimatorDriverSliderController : UdonSharpBehaviour
    {
        [SerializeField] public Animator animator;
        [SerializeField] public string parameterName;
        [SerializeField] public Slider[] sliders;

        [SerializeField]
        [UdonSynced]
        [FieldChangeCallback(nameof(Value))]
        private float _value = 0f;

        public float Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = value;
                foreach (Slider slider in sliders)
                {
                    if (slider.value != _value)
                    {
                        slider.SetValueWithoutNotify(_value);
                    }
                }
                animator.SetFloat(parameterName, _value);
            }
        }

        [SerializeField] public bool global = false;

        void Start()
        {
            animator.SetFloat(parameterName, _value);
            foreach (Slider slider in sliders)
            {
                if (slider.value != _value)
                {
                    slider.SetValueWithoutNotify(_value);
                }
            }
        }

        private void Sync()
        {
            if (!global) return;

            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
            RequestSerialization();
        }

        public void OnSliderValueChanged()
        {
            foreach (Slider slider in sliders)
            {
                if (slider.value != Value)
                {
                    Value = slider.value;
                    break;
                }
            }
            Sync();
        }

    }
}
