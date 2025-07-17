
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Playlist {

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayQueueItem : UdonSharpBehaviour
    {
        public PlaylistController controller;
        public int PlayID;

        public TextMeshProUGUI titleText;
        public TextMeshProUGUI artistText;
        public TextMeshProUGUI genreText;
        public TextMeshProUGUI userText;
        public string User;

        public VRCUrl PlayUrl;


        void Start()
        {

        }

        public void Setup(PlaylistController controller, int playID, string title, string artist, string genre, string user)
        {
            this.controller = controller;
            this.PlayID = playID;
            this.titleText.text = title;
            this.artistText.text = artist;
            this.genreText.text = genre;
            this.userText.text = user;
            this.User = user;
        }

        public void Setup(PlaylistController controller, VRCUrl playUrl, string user)
        {
            this.controller = controller;
            this.PlayID = -1;
            this.PlayUrl = playUrl;
            this.titleText.text = playUrl.Get();
            this.userText.text = user;
            this.User = user;
        }

        public void OnPlay()
        {
            controller.PlayFromQueueItem(this);
        }

        public void OnRemove()
        {
            controller.RemoveFromQueueSync(this);
        }

        public void OnMoveToTop()
        {
            controller.MoveToTopQueueSync(this);
        }
    }

}