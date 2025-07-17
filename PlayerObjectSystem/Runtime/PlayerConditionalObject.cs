
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PlayerConditionalObject : UdonSharpBehaviour
{

    public GameObject targetObject;

    public string[] conditionalPlayers;

    public string attachPlayerName;

    public HumanBodyBones attachPlayerBone;

    private VRCPlayerApi attachPlayer;

    private bool isConditionValid = false;

    private Vector3 initialPosition;

    private Quaternion initialRotation;

    void Start()
    {
        initialPosition = targetObject.transform.position;
        initialRotation = targetObject.transform.rotation;
        targetObject.SetActive(false);
    }

    void Update()
    {
        if (isConditionValid)
        {
            UpdateTargetObjectAttachment();
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (player.displayName == attachPlayerName)
            attachPlayer = player;

        if (isConditionValid)
            return;

        isConditionValid = ValidateCondition();

        if (isConditionValid)
            ActivateTargetObject();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (player == attachPlayer)
            attachPlayer = null;

        if (!isConditionValid)
            return;

        SendCustomEventDelayedFrames(nameof(OnPlayerLeftLateUpdate), 1, VRC.Udon.Common.Enums.EventTiming.LateUpdate);
    }

    public void OnPlayerLeftLateUpdate()
    {
        isConditionValid = ValidateCondition();

        if (!isConditionValid)
            ResetTargetObject();
    }

    private void ActivateTargetObject()
    {
        targetObject.SetActive(true);
    }

    private void ResetTargetObject()
    {
        targetObject.SetActive(false);
        SetTargetObjectTransform(initialPosition, initialRotation);
    }

    private void UpdateTargetObjectAttachment()
    {
        if (attachPlayer != null)
        {
            Vector3 targetPosition = attachPlayer.GetBonePosition(attachPlayerBone);
            Quaternion targetRotation = attachPlayer.GetBoneRotation(attachPlayerBone);

            SetTargetObjectTransform(targetPosition, targetRotation);
        }
    }

    private void SetTargetObjectTransform(Vector3 position, Quaternion rotation)
    {
        targetObject.transform.position = position;
        targetObject.transform.rotation = rotation;
    }

    private bool ValidateCondition()
    {
        VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        VRCPlayerApi.GetPlayers(players);

        string[] playerNames = new string[players.Length];
        for (int i = 0; i < players.Length; i++)
        {
            playerNames[i] = players[i].displayName;
        }

        for (int i = 0; i < conditionalPlayers.Length; i++)
        {
            if (System.Array.IndexOf(playerNames, conditionalPlayers[i]) == -1)
            {
                return false;
            }
        }
        return true;
    }

}