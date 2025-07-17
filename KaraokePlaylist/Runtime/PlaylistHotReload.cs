
using UdonSharp;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using VRC.SDKBase;
using VRC.Udon;

namespace Playlist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlaylistHotReload : UdonSharpBehaviour
    {
        public TMPro.TextMeshProUGUI versionText;

        private PlaylistController controller;
        private int localVersion = -1;

        [UdonSynced]
        private int globalVersion = -1;

        private bool isLoading = true;

        void Start()
        {

        }

        public void Setup(PlaylistController controller) {
            this.controller = controller;
        }

        public void SetLocalVersion(int version) {
            localVersion = version;
            if (localVersion > globalVersion) {
                RequestGlobalReload();
            }
            if (versionText != null)
                versionText.text = localVersion.ToString();
            isLoading = false;
        }

        private void RequestGlobalReload() {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            globalVersion = localVersion;

            RequestSerialization();
        }

        public override void OnDeserialization()
        {
            if (localVersion < globalVersion) {
                LocalReload();
            }
        }

        private void LocalReload()
        {
            if (isLoading) {
                return;
            }

            isLoading = true;
            controller.LoadPlaylist();
        }
    }

}