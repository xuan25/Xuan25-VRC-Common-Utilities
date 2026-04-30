#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnBuildHookUtility
{
    public class RemoveOnBuild : MonoBehaviour
    {
        public bool onDesktop = true;
        public bool onAndroid = true;
        public bool onIOS = true;
        public bool onVisionOS = true;
    }
}
#endif