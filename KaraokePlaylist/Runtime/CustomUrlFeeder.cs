
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace Playlist {

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CustomUrlFeeder : UdonSharpBehaviour
    {

        public PlaylistController controller;
        public VRCUrlInputField urlInputField;

        void Start()
        {

        }

        public override void Interact()
        {
            VRCUrl url = urlInputField.GetUrl();
            if (url == null || url.Get().Length == 0)
            {
                return;
            }

            Debug.Log("[Playlist] Adding to queue: " + urlInputField.GetUrl());
            controller.AddToQueueSync(urlInputField.GetUrl());

            urlInputField.SetUrl(VRCUrl.Empty);
        }
    }

}