
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.PlayerVoiceSystem
{
    public abstract class PlayerVoiceRoomBase : UdonSharpBehaviour
    {
        public abstract void Setup(PlayerVoiceRoomController controller, int roomID);
    }
}
