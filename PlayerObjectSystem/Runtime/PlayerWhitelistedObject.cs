
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PlayerWhitelistedObject : UdonSharpBehaviour
{
    public string[] whitelistedPlayers;
    public GameObject[] targetObjects;

    void Start()
    {
        string localPlayerName = Networking.LocalPlayer.displayName;
        bool isWhitelisted = false;
        foreach (string playerName in whitelistedPlayers)
        {
            if (playerName == localPlayerName)
            {
                isWhitelisted = true;
                break;
            }
        }
        foreach (GameObject targetObject in targetObjects)
        {
            targetObject.SetActive(isWhitelisted);
        }
    }
}
