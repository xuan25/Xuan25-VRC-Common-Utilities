#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace Xuan25.Internationalization.Editor
{

    using System.Collections.Generic;
    using UnityEngine;

    public class PortableObjectList : MonoBehaviour
    {
        public List<TextAsset> portableObjectFiles = new();

        public string childNamePrefix = "PortableObject_";

        public bool clearExistingChildren = true;

        [Header("Portable Object Handle Settings")]
        public string localeHeaderKey = "Language";

        public LocaleManager manager;
    }

}

#endif