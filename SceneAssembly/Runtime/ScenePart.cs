
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SceneAssembly
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ScenePart : UdonSharpBehaviour
    {
        public bool primary = false;

        public void Start()
        {
            // disable non primary scenes on initialization
            gameObject.SetActive(primary);
        }
    }

}