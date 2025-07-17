
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Playlist {

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlaylistItemMeta : UdonSharpBehaviour
    {
        public PlaylistController controller;
        public int PlayID;
        public string Title;
        public string Artist;
        public string TitleAcronym;
        public string ArtistAcronym;
        public string Genre;

        void Start()
        {

        }

        public void Setup(PlaylistController controller, int playID, string title, string artist, string titleAcronym, string singerAcronym, string genre)
        {
            this.controller = controller;
            this.PlayID = playID;
            this.Title = title;
            this.Artist = artist;
            this.TitleAcronym = titleAcronym;
            this.ArtistAcronym = singerAcronym;
            this.Genre = genre;
        }
    }

}
