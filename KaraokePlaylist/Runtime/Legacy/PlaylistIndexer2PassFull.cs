
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Playlist {

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlaylistIndexer2PassFull : UdonSharpBehaviour
    {

        public int itemsPreFrame = 50;

        public GameObject indexingHinter;

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
        private int playlistSortingCursor = -1;

        private bool isIndexing = false;
        
        
        void Start()
        {
            
        }

        public void Setup(PlaylistController controller)
        {
            playlistController = controller;
        }

        public void Update()
        {
            if (clearPlaylistCursor >= 0)
            {
                ClearPlaylistStep();
                return;
            }

            if (preparePlaylistCursor >= 0)
            {
                PreparePlaylistStep();
                return;
            }

            if (scoreComputingCursor >= 0)
            {
                ScoreComputingStep();
                return;
            }

            if (scoreSortingCursor >= 0)
            {
                ScoreSortingStep();
                return;
            }

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
            int targetPos = preparePlaylistCursor + itemsPreFrame;
            if (targetPos > playlistItemMetasCount)
            {
                targetPos = playlistItemMetasCount;
            }

            for (int i = preparePlaylistCursor; i < targetPos; i++)
            {
                PlaylistItemMeta playlistItemMeta = playlistItemMetas[playlistItemMetasCount - 1 - i];
                if (playlistItemMeta == null)
                {
                    continue;
                }

                GameObject playlistItem = playlistController.AppendItemToPlaylist(playlistItemMeta);
                playlistItems[i] = playlistItem;
            }

            preparePlaylistCursor = targetPos;

            if (targetPos >= playlistItemMetasCount) {
                preparePlaylistCursor = -1;
            }
        }

        private void ScoreComputingStep() {
            int targetPos = scoreComputingCursor + itemsPreFrame;
            if (targetPos > playlistItemMetasCount)
            {
                targetPos = playlistItemMetasCount;
            }

            for (int i = scoreComputingCursor; i < targetPos; i++)
            {
                PlaylistItemMeta playlistItemMeta = playlistItemMetas[playlistItemMetasCount - 1 - i];

                float filterScore = ComputeFilterScore(filterString, playlistItemMeta);
                
                if (filterScore <= 0)
                {
                    // invalid result penalty
                    filterScore -= 1000;
                    playlistItems[i].SetActive(false);
                }
                else {
                    // valid result
                    validResults++;
                    playlistItems[i].SetActive(true);
                }
                // Add sorting bias ranging from 0 to 1
                filterScore += (1 - ((float)i / playlistItemMetasCount)) * 0.001f;

                filterScores[i] = filterScore;
            }

            scoreComputingCursor = targetPos;

            // last score computing step
            if (targetPos >= playlistItemMetasCount) {
                scoreComputingCursor = -1;
            }
        }

        private void ScoreSortingStep() {
            // Sort the playlist items based on the filter scores
            Array.Copy(playlistItems, playlistItemsSorted, playlistItemMetasCount);
            Array.Sort((Array)filterScores, playlistItemsSorted);
            
            scoreSortingCursor = -1;
        }

        private void PlaylistSortingStep() {
            int targetPos = playlistSortingCursor + itemsPreFrame;
            if (targetPos > playlistItemMetasCount)
            {
                targetPos = playlistItemMetasCount;
            }

            for (int i = playlistSortingCursor; i < targetPos; i++)
            {
                GameObject playlistItem = playlistItemsSorted[playlistItemMetasCount - 1 - playlistSortingCursor];
                playlistItem.transform.SetSiblingIndex(i);
                playlistItem.SetActive(i < validResults);
                playlistSortingCursor++;
            }

            if (playlistSortingCursor >= playlistItemMetasCount)
            {
                playlistSortingCursor = -1;
                OnIndexingEnd();
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
        }

        public void PreparePlaylist()
        {
            Debug.Log("[PlaylistFilter] PreparePlaylist");

            playlistItemMetas = playlistController.GetPlaylistItemMetas();
            playlistItemMetasCount = playlistItemMetas.Length;

            filterScores = new float[playlistItemMetasCount];
            playlistItems = new GameObject[playlistItemMetasCount];
            playlistItemsSorted = new GameObject[playlistItemMetasCount];

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

        public void FilterPlaylist(string filter)
        {
            Debug.Log($"[PlaylistFilter] FilterPlaylist: {filter}");

            filterString = filter;
            scoreComputingCursor = 0;
            scoreSortingCursor = 0;
            playlistSortingCursor = 0;

            validResults = 0;

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
    }

}