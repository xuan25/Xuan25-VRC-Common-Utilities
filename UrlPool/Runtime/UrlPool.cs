
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class UrlPool : UdonSharpBehaviour
{
    [SerializeField]
    public VRCUrl[] Urls;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public VRCUrl GetUrl(int id)
    {
        if (id < 0 || id >= Urls.Length)
            return null;
        return Urls[id];
    }
    
}
