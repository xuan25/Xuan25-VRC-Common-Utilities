
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Playlist {

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlaylistIndexer : UdonSharpBehaviour
    {
        public int itemsPreFrame = 64;

        public GameObject indexingHinter;

        public int maxDisplayItems = 64;

        public Pager pager;

        private int itemDisplayOffset = 0;
        
        private PlaylistController playlistController;

        private int playlistItemMetasCount = 0;
        private PlaylistItemMeta[] playlistItemMetas;

        private GameObject[] playlistItems;
        private GameObject[] playlistItemsSorted;
        
        private float[] filterScores;

        private int validResults = 0;

        private string filterString = "";

        private int clearPlaylistCursor = -1;
        private int preparePlaylistCursor = -1;
        private int scoreComputingCursor = -1;
        private int scoreSortingCursor = -1;
        private int playlistHidingCursor = -1;
        private int playlistShowingCursor = -1;
        private int playlistSortingCursor = -1;

        private bool isIndexing = false;
        
        
        void Start()
        {
            pager.Setup(this);
        }

        public void Setup(PlaylistController controller)
        {
            playlistController = controller;
        }

        public void Update()
        {
            // loading clear
            if (clearPlaylistCursor >= 0)
            {
                ClearPlaylistStep();
                return;
            }

            // loading fill
            if (preparePlaylistCursor >= 0)
            {
                PreparePlaylistStep();
                return;
            }

            // searching metadata score compute
            if (scoreComputingCursor >= 0)
            {
                ScoreComputingStep();
                return;
            }

            // searching metadata score sort
            if (scoreSortingCursor >= 0)
            {
                ScoreSortingStep();
                return;
            }

            // searching UI hide pass
            if (playlistHidingCursor >= 0)
            {
                PlaylistHidingStep();
                return;
            }

            // searching UI show pass
            if (playlistShowingCursor >= 0)
            {
                PlaylistShowingStep();
                return;
            }

            // searching UI sorting pass
            if (playlistSortingCursor >= 0)
            {
                PlaylistSortingStep();
                return;
            }

            if (isIndexing)
            {
                OnIndexingEnd();
                return;
            }
        }

        private void ClearPlaylistStep() {
            for (int i = 0; i < itemsPreFrame; i++)
            {
                int childCount = playlistController.playlistItemContainer.childCount;
                if (childCount == 0)
                {
                    clearPlaylistCursor = -1;
                    break;
                }
                Transform child = playlistController.playlistItemContainer.GetChild(childCount - 1);
                child.SetParent(null);
                Destroy(child.gameObject);
            }
        }

        private void PreparePlaylistStep() {
            int stepActions = 0;
            while (stepActions < itemsPreFrame && preparePlaylistCursor < playlistItemMetasCount)
            {
                PlaylistItemMeta playlistItemMeta = playlistItemMetas[playlistItemMetasCount - 1 - preparePlaylistCursor];

                GameObject playlistItem = playlistController.AppendItemToPlaylist(playlistItemMeta);
                playlistItem.SetActive(preparePlaylistCursor >= itemDisplayOffset && preparePlaylistCursor < itemDisplayOffset + maxDisplayItems);
                playlistItems[preparePlaylistCursor] = playlistItem;
                playlistItemsSorted[playlistItemMetasCount - 1 - preparePlaylistCursor] = playlistItem;

                stepActions++;
                preparePlaylistCursor++;
            }

            if (preparePlaylistCursor >= playlistItemMetasCount) {
                validResults = playlistItemMetasCount;
                preparePlaylistCursor = -1;

                pager.Config(maxDisplayItems, playlistItemMetasCount);
            }
        }

        private void ScoreComputingStep() {
            int stepActions = 0;
            while (stepActions < itemsPreFrame && scoreComputingCursor < playlistItemMetasCount)
            {
                PlaylistItemMeta playlistItemMeta = playlistItemMetas[playlistItemMetasCount - 1 - scoreComputingCursor];
                float filterScore = ComputeFilterScore(filterString, playlistItemMeta);
                if (filterScore <= 0)
                {
                    // invalid result penalty
                    filterScore -= 1000;
                }
                else {
                    // valid result
                    validResults++;
                }
                filterScore += (1 - ((float)scoreComputingCursor / playlistItemMetasCount)) * 0.001f;
                filterScores[scoreComputingCursor] = filterScore;

                scoreComputingCursor++;
            }

            // last score computing step
            if (scoreComputingCursor >= playlistItemMetasCount) {
                scoreComputingCursor = -1;
                Debug.Log($"[PlaylistFilter] Score computing done, valid results: {validResults}");
            }
        }

        private void ScoreSortingStep() {
            // Sort the playlist items based on the filter scores
            Array.Copy(playlistItems, playlistItemsSorted, playlistItemMetasCount);
            Array.Sort((Array)filterScores, playlistItemsSorted);
            
            scoreSortingCursor = -1;
            Debug.Log("[PlaylistFilter] Score sorting done");
        }

        private void PlaylistHidingStep() {
            int stepActions = 0;
            while (stepActions < itemsPreFrame && playlistHidingCursor < playlistItemMetasCount)
            {
                GameObject playlistItem = playlistItemsSorted[playlistHidingCursor];
                int rank = playlistItemMetasCount - 1 - playlistHidingCursor;
                bool isVisible = rank < validResults && rank >= itemDisplayOffset && rank < itemDisplayOffset + maxDisplayItems;
                if (!isVisible && playlistItem.activeSelf)
                {
                    playlistItem.SetActive(false);
                    stepActions++;
                }
                playlistHidingCursor++;
            }

            // last score sorting step
            if (playlistHidingCursor >= playlistItemMetasCount)
            {
                playlistHidingCursor = -1;
                Debug.Log("[PlaylistFilter] Playlist hiding done");
            }
        }

        private void PlaylistShowingStep() {
            int stepActions = 0;
            while (stepActions < itemsPreFrame && playlistShowingCursor < playlistItemMetasCount)
            {
                GameObject playlistItem = playlistItemsSorted[playlistItemMetasCount - 1 - playlistShowingCursor];
                int rank = playlistShowingCursor;
                bool isVisible = rank < validResults && rank >= itemDisplayOffset && rank < itemDisplayOffset + maxDisplayItems;
                if (isVisible && !playlistItem.activeSelf)
                {
                    playlistItem.SetActive(true);
                    stepActions++;
                }
                playlistShowingCursor++;
            }

            // last score sorting step
            if (playlistShowingCursor >= playlistItemMetasCount)
            {
                playlistShowingCursor = -1;
                Debug.Log("[PlaylistFilter] Playlist Showing done");
            }
        }
        
        private void PlaylistSortingStep() {
            // sorting + showing
            int stepActions = 0;
            while (stepActions < itemsPreFrame && playlistSortingCursor < playlistItemMetasCount)
            {
                GameObject playlistItem = playlistItemsSorted[playlistItemMetasCount - 1 - playlistSortingCursor];
                int rank = playlistSortingCursor;
                bool isVisible = rank < validResults && rank >= itemDisplayOffset && rank < itemDisplayOffset + maxDisplayItems;
                if (isVisible && !playlistItem.activeSelf)
                {
                    playlistItem.SetActive(true);
                    playlistItem.transform.SetSiblingIndex(rank);
                    stepActions++;
                }
                playlistSortingCursor++;
            }

            // last score sorting step
            if (playlistSortingCursor >= playlistItemMetasCount)
            {
                playlistSortingCursor = -1;
                Debug.Log("[PlaylistFilter] Playlist Sorting done");
            }
        }

        private void OnIndexingBegin()
        {
            Debug.Log("[PlaylistFilter] OnIndexingBegin");

            isIndexing = true;

            if (indexingHinter != null)
            {
                indexingHinter.SetActive(true);
            }
        }

        private void OnIndexingEnd()
        {
            Debug.Log("[PlaylistFilter] OnIndexingEnd");
            isIndexing = false;

            if (indexingHinter != null)
            {
                indexingHinter.SetActive(false);
            }

            if (pager != null)
            {
                pager.Config(maxDisplayItems, validResults);
            }
        }

        public void PreparePlaylist()
        {
            Debug.Log("[PlaylistFilter] PreparePlaylist");

            playlistItemMetas = playlistController.GetPlaylistItemMetas();
            playlistItemMetasCount = playlistItemMetas.Length;

            filterScores = new float[playlistItemMetasCount];
            playlistItems = new GameObject[playlistItemMetasCount];
            playlistItemsSorted = new GameObject[playlistItemMetasCount];
            validResults = 0;
            itemDisplayOffset = 0;
            pager.Reset();

            clearPlaylistCursor = 0;
            preparePlaylistCursor = 0;

            OnIndexingBegin();

            // SYNCHRONOUS PREPARATION
            // for (int i = 0; i < playlistItemMetasCount; i++)
            // {
            //     PlaylistItemMeta playlistItemMeta = playlistItemMetas[playlistItemMetasCount - 1 - i];
            //     if (playlistItemMeta == null)
            //     {
            //         continue;
            //     }

            //     GameObject playlistItem = playlistController.AppendItemToPlaylist(playlistItemMeta);
            //     playlistItems[i] = playlistItem;
            // }
        }

        public void ReBuildPlaylist() {
            Debug.Log("[PlaylistFilter] ReBuildPlaylist");

            itemDisplayOffset = 0;
            pager.Reset();
            
            playlistHidingCursor = 0;
            playlistSortingCursor = 0;

            OnIndexingBegin();
        }

        public void FilterPlaylist(string filter)
        {
            Debug.Log($"[PlaylistFilter] FilterPlaylist: {filter}");

            validResults = 0;
            itemDisplayOffset = 0;
            pager.Reset();

            filterString = filter;
            scoreComputingCursor = 0;
            scoreSortingCursor = 0;
            playlistHidingCursor = 0;
            playlistSortingCursor = 0;

            OnIndexingBegin();

            
            // SYNCHRONOUS FILTERING

            // for (int i = 0; i < playlistItemMetasCount; i++)
            // {
            //     PlaylistItemMeta playlistItemMeta = playlistItemMetas[playlistItemMetasCount - 1 - i];
            //     if (playlistItemMeta == null)
            //     {
            //         continue;
            //     }

            //     float filterScore = ComputeFilterScore(filter, playlistItemMeta);

            //     if (filterScore <= 0)
            //     {
            //         // invalid result penalty
            //         filterScore -= 1000;
            //         playlistItems[i].SetActive(false);
            //     }
            //     else {
            //         // valid result
            //         validResults++;
            //         playlistItems[i].SetActive(true);
            //     }
            //     // Add sorting bias ranging from 0 to 1
            //     filterScore += 1 - ((float)i / playlistItemMetasCount);

            //     filterScores[i] = filterScore;
            // }


            // // Sort the playlist items based on the filter scores
            // Array.Copy(playlistItems, playlistItemsSorted, playlistItemMetasCount);
            // Array.Sort((Array)filterScores, playlistItemsSorted);
            

            // // Reorder the playlist items in the container
            // for (int i = 0; i < playlistItemMetasCount; i++)
            // {
            //     GameObject playlistItem = playlistItemsSorted[playlistItemMetasCount - 1 - i];
            //     playlistItem.transform.SetSiblingIndex(i);
            //     playlistItem.SetActive(i < validResults);
            // }


            // if (indexingHinter != null)
            // {
            //     indexingHinter.SetActive(false);
            // }
        }

        public void SetItemDisplayOffset(int itemDisplayOffset)
        {
            Debug.Log($"[PlaylistFilter] SetItemDisplayOffset: {itemDisplayOffset}");

            this.itemDisplayOffset = itemDisplayOffset;

            playlistHidingCursor = 0;
            playlistShowingCursor = 0;

            OnIndexingBegin();
        }

        private float ComputeFilterScore(string filter, PlaylistItemMeta playlistItemMeta)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return 1;
            }
            string[] tokens = filter.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            float score = 0;
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                if (playlistItemMeta.Title.Contains(token))
                {
                    score += (float)token.Length / playlistItemMeta.Title.Length;
                }
                if (playlistItemMeta.Artist.Contains(token))
                {
                    score += (float)token.Length / playlistItemMeta.Artist.Length;
                }
                if (playlistItemMeta.TitleAcronym.Contains(token))
                {
                    score += (float)token.Length / playlistItemMeta.TitleAcronym.Length;
                }
                if (playlistItemMeta.ArtistAcronym.Contains(token))
                {
                    score += (float)token.Length / playlistItemMeta.ArtistAcronym.Length;
                }
                if (playlistItemMeta.Genre.Contains(token))
                {
                    score += 1;
                }
            }

            return score;
        }

        public void StopOngoingTask() {
            Debug.Log("[PlaylistFilter] StopOngoingTask");

            scoreComputingCursor = -1;
            scoreSortingCursor = -1;
            playlistSortingCursor = -1;
        }

        public PlaylistItem GetRandomValidPlaylistItem()
        {
            if (playlistItemMetasCount == 0)
            {
                return null;
            }

            if (validResults == 0)
            {
                return null;
            }

            int randomIndex = UnityEngine.Random.Range(0, validResults);
            int playlistItemIndex = playlistItemMetasCount - 1 - randomIndex;
            GameObject playlistItem = playlistItemsSorted[playlistItemIndex];
            PlaylistItem playlistItemComponent = playlistItem.GetComponent<PlaylistItem>();
            return playlistItemComponent;
            
        }
    }

}