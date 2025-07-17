
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Playlist {

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlaylistIndexerHideMode : UdonSharpBehaviour
    {

        public int itemsPreFrame = 50;

        public GameObject indexingHinter;

        private PlaylistController playlistController;
        private string filterString = "";
        private int filterCursor = -1;

        private PlaylistItemMeta[] playlistItemMetas;
        private int playlistItemMetasCount = 0;
        
        void Start()
        {
            
        }

        public void Update()
        {
            if (filterCursor >= 0)
            {
                FilterStep();
                return;
            }
        }

        public void PreparePlaylist() {
            playlistItemMetas = playlistController.GetPlaylistItemMetas();
            playlistItemMetasCount = playlistItemMetas.Length;

            for (int i = 0; i < playlistItemMetasCount; i++)
            {
                PlaylistItemMeta playlistItemMeta = playlistItemMetas[playlistItemMetasCount - 1 - i];
                if (playlistItemMeta == null)
                {
                    continue;
                }

                playlistController.AppendItemToPlaylist(playlistItemMeta);
            }
        }

        private void FilterStep() {
            int targetPos = filterCursor + itemsPreFrame;
            if (targetPos > playlistItemMetasCount)
            {
                targetPos = playlistItemMetasCount;
            }

            for (int i = filterCursor; i < targetPos; i++)
            {
                PlaylistItemMeta playlistItemMeta = playlistItemMetas[playlistItemMetasCount - 1 - filterCursor];
                if (playlistItemMeta == null)
                {
                    filterCursor++;
                    continue;
                }

                if (!string.IsNullOrEmpty(filterString) && !playlistItemMeta.Title.Contains(filterString) && !playlistItemMeta.Artist.Contains(filterString) && !playlistItemMeta.TitleAcronym.Contains(filterString) && !playlistItemMeta.ArtistAcronym.Contains(filterString) && !playlistItemMeta.Genre.Contains(filterString))
                {
                    filterCursor++;
                    playlistController.playlistItemContainer.GetChild(i).gameObject.SetActive(false);
                    continue;
                }

                // playlistController.AppendItemToPlaylist(playlistItemMeta);
                playlistController.playlistItemContainer.GetChild(i).gameObject.SetActive(true);
                filterCursor++;
            }

            if (filterCursor >= playlistItemMetasCount)
            {
                OnFilterEnd();
            }
        }

        private void OnFilterEnd()
        {
            Debug.Log("[PlaylistFilter] OnFilterEnd");
            filterCursor = -1;

            if (indexingHinter != null)
            {
                indexingHinter.SetActive(false);
            }
        }

        public void Setup(PlaylistController controller)
        {
            playlistController = controller;
        }

        public void FilterPlaylist(string filter)
        {
            Debug.Log($"[PlaylistFilter] FilterPlaylist: {filter}");

            if (indexingHinter != null)
            {
                indexingHinter.SetActive(true);
            }
            
            StopOngoingTask();

            // ClearPlaylist();
            
            playlistItemMetas = playlistController.GetPlaylistItemMetas();
            playlistItemMetasCount = playlistItemMetas.Length;

            filterString = filter;
            filterCursor = 0;
        }

        public void StopOngoingTask() {
            filterCursor = -1;
        }

        public void ResetFilter() {
            Debug.Log($"[PlaylistFilter] ResetFilter");
            
            for (int i = 0; i < playlistItemMetasCount; i++)
            {
                PlaylistItemMeta playlistItemMeta = playlistItemMetas[playlistItemMetasCount - 1 - i];
                if (playlistItemMeta == null)
                {
                    continue;
                }
                playlistController.AppendItemToPlaylist(playlistItemMeta);
            }
        }

    }

}