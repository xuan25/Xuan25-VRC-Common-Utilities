
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

        public UnityEngine.UI.Slider progressBar;

        private int itemDisplayOffset = 0;
        
        private PlaylistController playlistController;

        private int playlistItemMetasCount = 0;
        private PlaylistItemMeta[] playlistItemMetas;

        private GameObject[] playlistItems;
        private GameObject[] playlistItemsSorted;
        
        private float[] filterScores;

        private int validResults = 0;

        private string[] filterTokens = null;

        const int FLAG_PENDING = 0;
        const int FLAG_FINISHED = -1;
        const int FLAG_INVALID = -2;

        private int clearPlaylistCursor = FLAG_INVALID;
        private int preparePlaylistCursor = FLAG_INVALID;
        private int scoreComputingCursor = FLAG_INVALID;
        private int scoreSortingCursor = FLAG_INVALID;
        private int playlistHidingCursor = FLAG_INVALID;
        private int playlistShowingCursor = FLAG_INVALID;
        private int playlistSortingCursor = FLAG_INVALID;

        private bool isIndexing = false;
        
        
        void Start()
        {
            if (indexingHinter != null)
            {
                indexingHinter.SetActive(false);
            }

            if (progressBar != null)
            {
                progressBar.value = 0;
                progressBar.gameObject.SetActive(false);
            }
            
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
                UpdateProgress();
                return;
            }

            // loading fill
            if (preparePlaylistCursor >= 0)
            {
                PreparePlaylistStep();
                UpdateProgress();
                return;
            }

            // searching metadata score compute
            if (scoreComputingCursor >= 0)
            {
                ScoreComputingStep();
                UpdateProgress();
                return;
            }

            // searching metadata score sort
            if (scoreSortingCursor >= 0)
            {
                ScoreSortingStep();
                UpdateProgress();
                return;
            }

            // searching UI hide pass
            if (playlistHidingCursor >= 0)
            {
                PlaylistHidingStep();
                UpdateProgress();
                return;
            }

            // searching UI show pass
            if (playlistShowingCursor >= 0)
            {
                PlaylistShowingStep();
                UpdateProgress();
                return;
            }

            // searching UI sorting pass
            if (playlistSortingCursor >= 0)
            {
                PlaylistSortingStep();
                UpdateProgress();
                return;
            }

            if (isIndexing)
            {
                UpdateProgress();
                OnIndexingEnd();
                return;
            }
        }

        private void UpdateProgress()
        {
            int[] cursors = new int [] { 
                clearPlaylistCursor, 
                preparePlaylistCursor, 
                scoreComputingCursor, 
                scoreSortingCursor, 
                playlistHidingCursor, 
                playlistShowingCursor, 
                playlistSortingCursor
            };
            int[] cursorsMax = new int [] {
                playlistItemMetasCount,
                playlistItemMetasCount,
                playlistItemMetasCount,
                playlistItemMetasCount,
                playlistItemMetasCount,
                playlistItemMetasCount,
                playlistItemMetasCount
            };
            int totalSteps = 0;
            int completedSteps = 0;
            float ratioOfCurrentStep = 0;

            for (int i = 0; i < cursors.Length; i++)
            {
                int cursor = cursors[i];
                int cursorMax = cursorsMax[i];
                if (cursor == FLAG_PENDING)
                {
                    totalSteps++;
                }
                else if (cursor == FLAG_FINISHED)
                {
                    totalSteps++;
                    completedSteps++;
                }
                else if (cursor >= 0)
                {
                    totalSteps++;
                    ratioOfCurrentStep = (float)cursor / cursorMax;
                    ratioOfCurrentStep = Mathf.Clamp(ratioOfCurrentStep, 0, 1);
                }
            }

            float progress = (float)completedSteps / totalSteps + ratioOfCurrentStep / totalSteps;

            Debug.Log($"[{nameof(PlaylistIndexer)}] Progress: {progress * 100}% ({completedSteps}/{totalSteps} + {ratioOfCurrentStep * 100}%)");

            if (progressBar != null)
            {
                progressBar.value = progress;
            }
        }

        private void ClearPlaylistStep() {
            for (int i = 0; i < itemsPreFrame; i++)
            {
                int childCount = playlistController.playlistItemContainer.childCount;
                if (childCount == 0)
                {
                    clearPlaylistCursor = FLAG_FINISHED;
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
                preparePlaylistCursor = FLAG_FINISHED;

                pager.Config(maxDisplayItems, playlistItemMetasCount);
            }
        }

        private void ScoreComputingStep() {
            int stepActions = 0;
            while (stepActions < itemsPreFrame && scoreComputingCursor < playlistItemMetasCount)
            {
                PlaylistItemMeta playlistItemMeta = playlistItemMetas[playlistItemMetasCount - 1 - scoreComputingCursor];
                float filterScore = ComputeFilterScore(filterTokens, playlistItemMeta);
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
                scoreComputingCursor = FLAG_FINISHED;
                Debug.Log($"[PlaylistFilter] Score computing done, valid results: {validResults}");
            }
        }

        private void ScoreSortingStep() {
            // Sort the playlist items based on the filter scores
            Array.Copy(playlistItems, playlistItemsSorted, playlistItemMetasCount);
            Array.Sort((Array)filterScores, playlistItemsSorted);
            
            scoreSortingCursor = FLAG_FINISHED;
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
                playlistHidingCursor = FLAG_FINISHED;
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
                playlistShowingCursor = FLAG_FINISHED;
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
                playlistSortingCursor = FLAG_FINISHED;
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

            if (progressBar != null)
            {
                progressBar.value = 0;
                progressBar.gameObject.SetActive(true);
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

            if (progressBar != null)
            {
                progressBar.value = 1;
                progressBar.gameObject.SetActive(false);
            }

            if (pager != null)
            {
                pager.Config(maxDisplayItems, validResults);
            }
        }

        private void InvalidateCursorsExceptPrepare()
        {
            scoreComputingCursor = FLAG_INVALID;
            scoreSortingCursor = FLAG_INVALID;
            playlistHidingCursor = FLAG_INVALID;
            playlistShowingCursor = FLAG_INVALID;
            playlistSortingCursor = FLAG_INVALID;
        }

        public void PreparePlaylist()
        {
            Debug.Log("[PlaylistFilter] PreparePlaylist");

            InvalidateCursorsExceptPrepare();

            playlistItemMetas = playlistController.GetPlaylistItemMetas();
            playlistItemMetasCount = playlistItemMetas.Length;

            filterScores = new float[playlistItemMetasCount];
            playlistItems = new GameObject[playlistItemMetasCount];
            playlistItemsSorted = new GameObject[playlistItemMetasCount];
            validResults = 0;
            itemDisplayOffset = 0;
            pager.Reset();

            clearPlaylistCursor = FLAG_PENDING;
            preparePlaylistCursor = FLAG_PENDING;

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

            InvalidateCursorsExceptPrepare();

            itemDisplayOffset = 0;
            pager.Reset();
            
            playlistHidingCursor = FLAG_PENDING;
            playlistSortingCursor = FLAG_PENDING;

            OnIndexingBegin();
        }

        public void FilterPlaylist(string filter)
        {
            Debug.Log($"[PlaylistFilter] FilterPlaylist: {filter}");

            InvalidateCursorsExceptPrepare();

            validResults = 0;
            itemDisplayOffset = 0;
            pager.Reset();

            filterTokens = string.IsNullOrEmpty(filter) ? null : filter.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            scoreComputingCursor = FLAG_PENDING;
            scoreSortingCursor = FLAG_PENDING;
            playlistHidingCursor = FLAG_PENDING;
            playlistSortingCursor = FLAG_PENDING;

            OnIndexingBegin();

            
            // SYNCHRONOUS FILTERING

            // for (int i = 0; i < playlistItemMetasCount; i++)
            // {
            //     PlaylistItemMeta playlistItemMeta = playlistItemMetas[playlistItemMetasCount - 1 - i];
            //     if (playlistItemMeta == null)
            //     {
            //         continue;
            //     }

            //     float filterScore = ComputeFilterScore(filterTokens, playlistItemMeta);

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

            InvalidateCursorsExceptPrepare();

            playlistHidingCursor = FLAG_PENDING;
            playlistShowingCursor = FLAG_PENDING;

            OnIndexingBegin();
        }

        private float ComputeFilterScore(string[] filterTokens, PlaylistItemMeta playlistItemMeta)
        {
            if (filterTokens == null || filterTokens.Length == 0)
            {
                return 1;
            }
            string[] tokens = filterTokens;
            float score = 0;
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                if (playlistItemMeta.TitleUpper.Contains(token))
                {
                    score += (float)token.Length / playlistItemMeta.TitleUpper.Length;
                }
                if (playlistItemMeta.ArtistUpper.Contains(token))
                {
                    score += (float)token.Length / playlistItemMeta.ArtistUpper.Length;
                }
                if (playlistItemMeta.TitleAcronymUpper.Contains(token))
                {
                    score += (float)token.Length / playlistItemMeta.TitleAcronymUpper.Length;
                }
                if (playlistItemMeta.ArtistAcronymUpper.Contains(token))
                {
                    score += (float)token.Length / playlistItemMeta.ArtistAcronymUpper.Length;
                }
                if (playlistItemMeta.GenreUpper.Contains(token))
                {
                    score += 1;
                }
            }

            return score;
        }

        public void StopOngoingTask() {
            Debug.Log("[PlaylistFilter] StopOngoingTask");

            InvalidateCursorsExceptPrepare();
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