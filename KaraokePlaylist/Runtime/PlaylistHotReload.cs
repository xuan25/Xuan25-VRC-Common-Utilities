
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

        public GameObject reloadButton;

        public bool allowAutoReload = false;

        private PlaylistController controller;
        private long localVersion = -1;

        [UdonSynced]
        private long globalVersion = -1;

        void Start()
        {

        }

        public void Setup(PlaylistController controller) {
            this.controller = controller;
        }

        public void SetLocalVersion(long version) {
            localVersion = version;

            // Broadcast the new version to other clients if it's newer than the global version
            if (localVersion > globalVersion) {
                RequestGlobalReload();
            }

            // Update the version text for debugging purposes
            if (versionText != null)
                versionText.text = localVersion.ToString();

            // Set reload button visibility based on version comparison
            if (reloadButton != null) {
                if (localVersion < globalVersion) {
                    reloadButton.SetActive(true);
                } else {
                    reloadButton.SetActive(false);
                }
            }
        }

        private void RequestGlobalReload() {
            // Update the global version and request serialization to notify other clients
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            globalVersion = localVersion;

            RequestSerialization();
        }

        public override void OnDeserialization()
        {
            if (localVersion < globalVersion) {
                if (allowAutoReload) {
                    LocalReload();
                    return;
                }
                if (reloadButton != null) {
                    reloadButton.SetActive(true);
                }
            }
        }

        public void OnReloadButtonPressed() {
            if (!this.controller.GetIsPlaylistLoaded()) {
                return;
            }

            if (reloadButton != null) {
                reloadButton.SetActive(false);
            }
            
            controller.LoadPlaylist();
        }

        private void LocalReload()
        {
            if (!this.controller.GetIsPlaylistLoaded()) {
                return;
            }

            controller.LoadPlaylist();
        }
    }

}