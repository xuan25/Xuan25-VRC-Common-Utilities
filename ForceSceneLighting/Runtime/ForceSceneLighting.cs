using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ForceSceneLighting
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class ForceSceneLighting : UdonSharpBehaviour
    {
        private int idUdonForceSceneLighting = -1;

        private bool isInitialized = false;

        public void OnEnable()
        {
            if (!isInitialized)
            {
                idUdonForceSceneLighting = VRCShader.PropertyToID("_UdonForceSceneLighting");
                isInitialized = true;
            }
            VRCShader.SetGlobalFloat(idUdonForceSceneLighting, 1);
        }

        public void OnDisable()
        {
            VRCShader.SetGlobalFloat(idUdonForceSceneLighting, 0);
        }
    }
}