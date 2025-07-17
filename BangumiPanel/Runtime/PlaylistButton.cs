
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace BangumiPanel
{

    public class PlaylistButton : UdonSharpBehaviour
    {
    public PlaylistManager playlistLoader;
    public string role;
    public int index;

    void Start()
    {
        
    }

    public void Setup(PlaylistManager loader, string name, string role, int index)
    {
        this.playlistLoader = loader;
        this.role = role;
        this.index = index;
        GetComponentInChildren<TMPro.TextMeshProUGUI>().text = name;
    }

    public void OnClick()
    {
        Debug.Log("Button clicked");
        playlistLoader.UpdateSelection(role, index);
    }
    }

}
