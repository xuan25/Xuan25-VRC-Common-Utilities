
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace AnimatorUtilities
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class DiscreteAnimatorDriver : UdonSharpBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string parameterName;
        [SerializeField] private AnimatorControllerParameterType parameterType;
        [SerializeField] private float[] parameterFloatValue;
        [SerializeField] private int[] parameterIntValue;
        [SerializeField] private bool[] parameterBoolValue;
        [SerializeField] private bool resetOnStart = true;
        [SerializeField] private bool global;

        [UdonSynced]
        private int index = -1;

        void Start()
        {
            ResetOnStartIfNeeded();
        }

        private void ResetOnStartIfNeeded()
        {
            // Do nothing if not global, or if we are the owner (in a global synced setup))
            if (global && !Networking.IsOwner(gameObject)) return;
            // Do nothing if we are not supposed to reset on start
            if (!resetOnStart) return;
            // Reset to initial state (index 0)
            SetParameter(0);
        }

        private int GetParameterArrayLength()
        {
            switch (parameterType)
            {
                case AnimatorControllerParameterType.Float:
                    return parameterFloatValue.Length;
                case AnimatorControllerParameterType.Int:
                    return parameterIntValue.Length;
                case AnimatorControllerParameterType.Bool:
                    return parameterBoolValue.Length;
                case AnimatorControllerParameterType.Trigger:
                    return 1; // Trigger doesn't have an array, just one action
                default:
                    Debug.LogError("Unsupported parameter type.");
                    return 0;
            }
        }

        public void SetParameter()
        {
            SetParameter((index + 1) % GetParameterArrayLength());
        }

        public void SetParameter(int index)
        {
            this.index = index;
            UpdateStateLocal();
            if (global)
            {
                if (!Networking.IsOwner(gameObject))
                {
                    Networking.SetOwner(Networking.LocalPlayer, gameObject);
                }
                RequestSerialization();
            }
        }

        private void UpdateStateLocal()
        {
            switch (parameterType)
            {
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(parameterName, parameterFloatValue[index]);
                    break;
                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(parameterName, parameterIntValue[index]);
                    break;
                case AnimatorControllerParameterType.Bool:
                    animator.SetBool(parameterName, parameterBoolValue[index]);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    animator.SetTrigger(parameterName);
                    break;
                default:
                    Debug.LogError("Unsupported parameter type.");
                    break;
            }
        }

        public override void OnDeserialization()
        {
            UpdateStateLocal();
        }

        public override void Interact()
        {
            SetParameter();
        }
    }
}