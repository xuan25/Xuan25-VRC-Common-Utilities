
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Playlist {

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlaylistIndexerObjMode : UdonSharpBehaviour
    {

        public int itemsPreFrame = 50;

        public GameObject indexingHinter;

        private PlaylistController playlistController;
        private string filterString = "";
        private int filterCursor = -1;

        private PlaylistItemMeta[] playlistItemMetas;
        private int playlistItemCount = 0;

        private bool isClearingPlaylist = false;
        
        void Start()
        {
            
        }

        public void Update()
        {
            if (isClearingPlaylist)
            {
                ClearStep();
                return;
            }

            if (filterCursor >= 0)
            {
                FilterStep();
                return;
            }
        }

        public void PreparePlaylist() {
            FilterPlaylist(string.Empty);
        }

        private void FilterStep() {
            int targetPos = filterCursor + itemsPreFrame;
            if (targetPos > playlistItemCount)
            {
                targetPos = playlistItemCount;
            }

            for (int i = filterCursor; i < targetPos; i++)
            {
                PlaylistItemMeta playlistItemMeta = playlistItemMetas[playlistItemCount - 1 - filterCursor];
                if (playlistItemMeta == null)
                {
                    filterCursor++;
                    continue;
                }

                if (!string.IsNullOrEmpty(filterString) && !playlistItemMeta.Title.Contains(filterString) && !playlistItemMeta.Artist.Contains(filterString) && !playlistItemMeta.TitleAcronym.Contains(filterString) && !playlistItemMeta.ArtistAcronym.Contains(filterString) && !playlistItemMeta.Genre.Contains(filterString))
                {
                    filterCursor++;
                    continue;
                }

                playlistController.AppendItemToPlaylist(playlistItemMeta);
                filterCursor++;
            }

            if (filterCursor >= playlistItemCount)
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

        private void ClearStep() {
            for (int i = 0; i < itemsPreFrame; i++)
            {
                int childCount = playlistController.playlistItemContainer.childCount;
                if (childCount == 0)
                {
                    isClearingPlaylist = false;
                    break;
                }
                Transform child = playlistController.playlistItemContainer.GetChild(childCount - 1);
                child.SetParent(null);
                Destroy(child.gameObject);
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

            ClearPlaylist();
            
            playlistItemMetas = playlistController.GetPlaylistItemMetas();
            playlistItemCount = playlistItemMetas.Length;

            filterString = filter;
            filterCursor = 0;
        }

        public void StopOngoingTask() {
            filterCursor = -1;
        }

        public void ResetFilter() {
            Debug.Log($"[PlaylistFilter] ResetFilter");
            
            for (int i = 0; i < playlistItemCount; i++)
            {
                PlaylistItemMeta playlistItemMeta = playlistItemMetas[playlistItemCount - 1 - i];
                if (playlistItemMeta == null)
                {
                    continue;
                }
                playlistController.AppendItemToPlaylist(playlistItemMeta);
            }
        }

        internal void ClearPlaylist()
        {
            Debug.Log("[PlaylistFilter] ClearPlaylist");
            isClearingPlaylist = true;
        }

    }

}