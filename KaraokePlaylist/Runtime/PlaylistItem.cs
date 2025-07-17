
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Playlist {

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlaylistItem : UdonSharpBehaviour
    {
        public PlaylistController controller;
        public int PlayID;

        public TextMeshProUGUI titleText;
        public TextMeshProUGUI artistText;
        public TextMeshProUGUI genreText;

        void Start()
        {

        }

        public void Setup(PlaylistController controller, int playID, string title, string artist, string genre)
        {
            this.controller = controller;
            PlayID = playID;
            titleText.text = title;
            artistText.text = artist;
            genreText.text = genre;
        }

        public void OnQueue()
        {
            controller.AddToQueueSync(PlayID);
        }
    }

}
