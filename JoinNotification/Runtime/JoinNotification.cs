
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class JoinNotification : UdonSharpBehaviour
{

    public Animator animator;

    public AudioSource audioSourceJoin;

    public AudioSource audioSourceLeft;

    void Start()
    {

    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (animator != null)
        {
            animator.SetTrigger("OnJoint");
        }
        if (audioSourceJoin != null)
        {
            audioSourceJoin.Play();
        }
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (animator != null)
        {
            animator.SetTrigger("OnLeft");
        }
        if (audioSourceLeft != null)
        {
            audioSourceLeft.Play();
        }
    }
}
