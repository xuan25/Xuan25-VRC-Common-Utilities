
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

        public string TitleUpper;
        public string ArtistUpper;
        public string TitleAcronymUpper;
        public string ArtistAcronymUpper;
        public string GenreUpper;

        void Start()
        {

        }

        public void Setup(PlaylistController controller, int playID, string title, string artist, string titleAcronym, string singerAcronym, string genre)
        {
            this.controller = controller;
            this.PlayID = playID;
            this.Title = title;
            this.TitleUpper = title.ToUpper();
            this.Artist = artist;
            this.ArtistUpper = artist.ToUpper();
            this.TitleAcronym = titleAcronym;
            this.TitleAcronymUpper = titleAcronym.ToUpper();
            this.ArtistAcronym = singerAcronym;
            this.ArtistAcronymUpper = singerAcronym.ToUpper();
            this.Genre = genre;
            this.GenreUpper = genre.ToUpper();
        }
    }

}
