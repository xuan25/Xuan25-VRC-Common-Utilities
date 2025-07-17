
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Playlist {

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class VideoPlayerSyncHandler : UdonSharpBehaviour
    {
        private PlaylistController playlistController;

        [UdonSynced]
        private string title;
        [UdonSynced]
        private string author;

        public void Setup(PlaylistController controller)
        {
            playlistController = controller;
        }

        public void SetMetadata(string title, string author)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

            this.title = title;
            this.author = author;

            RequestSerialization();

#if VIZVID
            playlistController.playerCore.SetTitle(title, author);
#endif
        }

        public override void OnDeserialization()
        {
#if VIZVID
            playlistController.playerCore.SetTitle(title, author);
#endif
        }
    }

}