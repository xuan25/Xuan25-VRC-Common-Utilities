using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Data;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace Playlist {

    [DefaultExecutionOrder(-100)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlaylistController : UdonSharpBehaviour
    {
        #if VIZVID
        public JLChnToZ.VRC.VVMW.Core playerCore;
        #endif
        public AudioStateController audioStateController;
        public VRCUrl playlistEndpoint;
        public UrlPool urlPool;
        public GameObject searchInputField;
        public Transform playlistItemContainer;
        public Transform playQueueItemContainer;
        public Transform playlistItemMetaContainer;
        public GameObject playlistItemTemplate;
        public GameObject playQueueItemTemplate;
        public GameObject playlistItemMetaTemplate;
        public GameObject overlayLoading;
        public GameObject overlayError;
        #if VIZVID
        public VizVidTitleSync vizVidTitleSync;
        #endif
        public PlaylistIndexer playlistIndexer;
        public PlaylistHotReload playlistHotReload;

        public PlaylistItemMeta[] playlistItemMetas;

        public int[] playIDCompactIdxMap;

        public PlaylistItemMeta[] GetPlaylistItemMetas()
        {
            return playlistItemMetas;
        }

        public int queueSizeMax = 64;

        #if VIZVID
        public string playerName = "AvProPlayer";
        #endif

        [UdonSynced]
        private int[] queuePlayIDs;

        [UdonSynced]
        private VRCUrl[] queuePlayUrls;

        [UdonSynced]
        private string[] queueUsers;

        [UdonSynced]
        private int queueSize = 0;

        private bool isPlayerBusy = false;

        #if VIZVID
        private byte playerType = 0;
        #endif

        private bool isPlaylistLoaded = false;

        public bool GetIsPlaylistLoaded()
        {
            return isPlaylistLoaded;
        }

        void Start()
        {
#if UNITY_EDITOR
#if VIZVID
            playerName = "BuiltInPlayer";
            Debug.Log($"[Playlist] Player name overrides to {playerName} within editor."); 
#endif
#endif
#if VIZVID
            vizVidTitleSync.Setup(this);
#endif
            playlistIndexer.Setup(this);
            playlistHotReload.Setup(this);

            // TODO: re-implement with "list"
            queuePlayIDs = new int[queueSizeMax];
            queuePlayUrls = new VRCUrl[queueSizeMax];
            for (int i = 0; i < queueSizeMax; i++)
            {
                queuePlayUrls[i] = VRCUrl.Empty;
            }
            queueUsers = new string[queueSizeMax];
            for (int i = 0; i < queueSizeMax; i++)
            {
                queueUsers[i] = string.Empty;
            }

#if VIZVID
            for (byte i = 0; i < playerCore.PlayerNames.Length; i++)
            {
                if (playerCore.PlayerNames[i] == playerName)
                {
                    playerType = (byte)(i + 1);
                    Debug.Log($"[Playlist] Player type: {playerType}");
                    break;
                }
            }
            if (playerType == 0)
            {
                Debug.LogError($"[Playlist] Player type not found: {playerName}");
            }
#endif

            LoadPlaylist();
        }

        public void LoadPlaylist()
        {
            isPlaylistLoaded = false;
            Debug.Log($"[Playlist] LoadPlaylist: {playlistEndpoint}");
            playlistIndexer.StopOngoingTask();
            overlayLoading.SetActive(true);
            overlayError.SetActive(false);
            VRCStringDownloader.LoadUrl(playlistEndpoint, (IUdonEventReceiver)this);
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            Debug.Log($"[Playlist] OnStringLoadSuccess");
            string resultAsUTF8 = result.Result;

            LoadPlaylist(resultAsUTF8);

            // playlistFilter.FilterPlaylist(string.Empty);
            playlistIndexer.PreparePlaylist();
            // FilterPlaylist(string.Empty);

            isPlaylistLoaded = true;
            
            RebuildQueueDisplay();

            overlayLoading.SetActive(false);
            overlayError.SetActive(false);
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            Debug.LogError($"[Playlist] OnStringLoadError: {result.ErrorCode} - {result.Error}");
            overlayLoading.SetActive(false);
            overlayError.SetActive(true);
        }

        private void LoadPlaylist(string json)
        {
            Debug.Log($"[Playlist] LoadPlaylist");
            foreach (Transform child in playlistItemContainer.transform)
            {
                Destroy(child.gameObject);
            }

            if (!VRCJson.TryDeserializeFromJson(json, out DataToken result))
            {
                Debug.Log($"[Playlist] Failed to Deserialize json {json} - {result}");
                return;
            }

            // Assume the playlist is well-formed and all data lists are of the same length as COUNT.
            // If IS_IN_USE and NUM_IN_USE are present, initialize playlist items based on IS_IN_USE.
            // Otherwise, initialize all playlist items and assume all are in use.
            int count = (int)result.DataDictionary["COUNT"].Double;

            bool hasIsInUse = result.DataDictionary.ContainsKey("IS_IN_USE") && result.DataDictionary.ContainsKey("NUM_IN_USE");
            DataList isInUse = hasIsInUse ? result.DataDictionary["IS_IN_USE"].DataList : null;
            int numInUse = hasIsInUse ? (int)result.DataDictionary["NUM_IN_USE"].Double : count;

            playlistItemMetas = new PlaylistItemMeta[numInUse];
            playIDCompactIdxMap = new int[count];

            DataList titles = result.DataDictionary["TITLE"].DataList;
            DataList artists = result.DataDictionary["ARTIST"].DataList;
            DataList titleAcronyms = result.DataDictionary["TITLE_ACRONYM"].DataList;
            DataList artistAcronyms = result.DataDictionary["ARTIST_ACRONYM"].DataList;
            DataList genres = result.DataDictionary["GENRE"].DataList;

            // Generate playIDs and sort by order if ORDER data is available
            int[] playIDs = new int[count];
            for (int i = 0; i < count; i++) {
                playIDs[i] = i;
            }
            if (result.DataDictionary.ContainsKey("ORDER"))
            {
                DataList orderList = result.DataDictionary["ORDER"].DataList;
                int[] orders = new int[count];
                
                for (int i = 0; i < count; i++) {
                    orders[i] = (int)orderList[i].Double;
                }

                Array.Sort((Array)orders, playIDs);
                Debug.Log($"[Playlist] Sorted playlist by order");
            }

            // Enum through playIDs in sorted order, and only generate playlist items for those in use if in use data is available.
            // Map playID to compact index for later retrieval when playing.
            int compactIdx = 0;
            for (int i = 0; i < count; i++)
            {
                int playID = playIDs[i];
                if (hasIsInUse)
                {
                    if (!isInUse[playID].Boolean)
                    {
                        playIDCompactIdxMap[playID] = -1;
                        Debug.Log($"[Playlist] Skipping unused playlist item {playID} {titles[playID].String}");
                        continue;
                    }
                }
                GameObject playlistItemMetaObject = Instantiate(playlistItemMetaTemplate, playlistItemMetaContainer);
                PlaylistItemMeta playlistItemMeta = playlistItemMetaObject.GetComponentInChildren<PlaylistItemMeta>();

                playlistItemMeta.Setup(this, playID,
                    titles[playID].String,
                    artists[playID].String,
                    titleAcronyms[playID].String,
                    artistAcronyms[playID].String,
                    genres[playID].String
                );

                playlistItemMetas[compactIdx] = playlistItemMeta;
                playIDCompactIdxMap[playID] = compactIdx;

                compactIdx++;
            }

            // Set the local version to the version in the playlist data, 
            // so that hot reload can work correctly after the initial load.
            long version = (long)result.DataDictionary["VERSION"].Double;
            playlistHotReload.SetLocalVersion(version);

            Debug.Log($"[Playlist] Loaded {numInUse} out of {count} playlist items");
        }

        public void OnSearch()
        {
            Debug.Log("[Playlist] OnSearch");
            // FilterPlaylist(searchInputField.GetComponent<TMPro.TMP_InputField>().text.ToUpper());
            playlistIndexer.FilterPlaylist(searchInputField.GetComponent<TMPro.TMP_InputField>().text.ToUpper());
        }

        // private void FilterPlaylist(string keywords)
        // {
        //     Debug.Log($"[Playlist] FilterPlaylist: {keywords}");
        //     ClearPlaylist();

        //     for (int i = 0; i < playlistItemMetas.Length; i++)
        //     {
        //         PlaylistItemMeta playlistItemMeta = playlistItemMetas[i];
        //         if (!string.IsNullOrEmpty(keywords) && !playlistItemMeta.Title.Contains(keywords) && !playlistItemMeta.Artist.Contains(keywords) && !playlistItemMeta.TitleAcronym.Contains(keywords) && !playlistItemMeta.ArtistAcronym.Contains(keywords) && !playlistItemMeta.Genre.Contains(keywords))
        //         {
        //             continue;
        //         }
        //         AddItemToPlaylist(playlistItemMeta);
        //     }
        // }

        internal GameObject AppendItemToPlaylist(PlaylistItemMeta playlistItemMeta)
        {
            GameObject playlistItem = Instantiate(playlistItemTemplate, playlistItemContainer.transform);
            playlistItem.GetComponent<PlaylistItem>().Setup(this, playlistItemMeta.PlayID, playlistItemMeta.Title, playlistItemMeta.Artist, playlistItemMeta.Genre);
            return playlistItem;
        }

        // internal void ClearPlaylist()
        // {
        //     Debug.Log("[Playlist] ClearPlaylist");
        //     foreach (Transform child in playlistItemContainer.transform)
        //     {
        //         Destroy(child.gameObject);
        //     }
        // }

        public override void OnVideoReady()
        {
            Debug.Log("[Playlist] OnVideoReady");
            isPlayerBusy = true;
        }

        public override void OnVideoPlay()
        {
            Debug.Log("[Playlist] OnVideoPlay");
            isPlayerBusy = true;
        }

        public override void OnVideoStart()
        {
            Debug.Log("[Playlist] OnVideoStart");
            isPlayerBusy = true;
        }

        public override void OnVideoPause()
        {
            Debug.Log("[Playlist] OnVideoPause");
            isPlayerBusy = true;
        }

        public override void OnVideoLoop()
        {
            Debug.Log("[Playlist] OnVideoLoop");
            isPlayerBusy = true;
        }

        public override void OnVideoError(VideoError videoError)
        {
            Debug.Log("[Playlist] OnVideoError");
            isPlayerBusy = false;
        }

        public override void OnVideoEnd()
        {
            Debug.Log("[Playlist] OnVideoEnd");
            isPlayerBusy = false;

            // PlayNext();
            SendCustomEventDelayedFrames(nameof(PlayNext), 1, VRC.Udon.Common.Enums.EventTiming.LateUpdate);
        }

        private void PlayByID(int playID, string user)
        {
#if VIZVID
            playerCore.PlayUrl(urlPool.Urls[playID], playerType);
            int compactIdx = playIDCompactIdxMap[playID];
            if (compactIdx < 0)
            {
                Debug.LogError($"[Playlist] PlayByID: playID {playID} is not in use.");
                vizVidTitleSync.SetMetadata($"[{user}] Unknown Title", "Unknown Artist");
                return;
            }
            vizVidTitleSync.SetMetadata($"[{user}] {playlistItemMetas[compactIdx].Title}", $"{playlistItemMetas[compactIdx].Artist}");
#endif

            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(OnPlayScheduled));
            isPlayerBusy = true;
        }

        private void PlayByUrl(VRCUrl playUrl, string user)
        {
#if VIZVID
            playerCore.PlayUrl(playUrl, playerType);
            vizVidTitleSync.SetMetadata($"[{user}]", string.Empty);
#endif

            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(OnPlayScheduled));
            isPlayerBusy = true;
        }

        public void OnPlayScheduled()
        {
            isPlayerBusy = true;
        }

        private VRCPlayerApi GetPlayerByName(string playerName)
        {
            VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];  
            VRCPlayerApi.GetPlayers(players);
            foreach (VRCPlayerApi player in players)
            {
                if (player.displayName == playerName)
                {
                    return player;
                }
            }
            return null;
        }

        public void PlayNext()
        {
            Debug.Log("[Playlist] PlayNext");
            if (playQueueItemContainer.childCount <= 0)
            {
                return;
            }

            PlayQueueItem playQueueItem = playQueueItemContainer.GetChild(0).GetComponent<PlayQueueItem>();
            
            // If the player who added the item to the queue is in the instance, they should play that item.
            if (playQueueItem.User == Networking.LocalPlayer.displayName)
            {
                // player who added the item to the queue is in the instance, play the item
                PlayFromQueueItem(playQueueItem);
                return;
            }

            // Current owner checks if the player who added the item to the queue is in the instance.
            // If the player who added the item to the queue is not in the instance, current owner should play the item.
            if (Networking.IsOwner(gameObject)) {
                VRCPlayerApi player = GetPlayerByName(playQueueItem.User);
                if (player != null)
                {
                    // player is in the instance, do not play the item
                    return;
                }
                // player is not in the instance, current owner should play the item
                PlayFromQueueItem(playQueueItem);
            }
        }

        public void PlayFromQueueItem(PlayQueueItem playQueueItem)
        {
            Debug.Log($"[Playlist] Play from queue: {playQueueItem.PlayID}");
            if (playQueueItem.PlayID < 0)
            {
                PlayByUrl(playQueueItem.PlayUrl, playQueueItem.User);
            }
            else
            {
                PlayByID(playQueueItem.PlayID, playQueueItem.User);
            }
            RemoveFromQueueSync(playQueueItem);
        }

        public void AddToQueueSync(VRCUrl playUrl)
        {
            Debug.Log($"[Playlist] Add to queue: {playUrl}");

            if (playQueueItemContainer.childCount == 0 && !isPlayerBusy)
            {
                PlayByUrl(playUrl, Networking.LocalPlayer.displayName);
                return;
            }

            GameObject playlistItem = Instantiate(playQueueItemTemplate, playQueueItemContainer);
            playlistItem.GetComponent<PlayQueueItem>().Setup(this, playUrl, Networking.LocalPlayer.displayName);

            SyncQueue();
        }

        public void AddRandomToQueueSync()
        {
            Debug.Log("[Playlist] Add random to queue");
            // if (playlistItemContainer.childCount == 0)
            // {
            //     Debug.LogWarning("[Playlist] No items in playlist. Cannot add random item to queue.");
            //     return;
            // }
            // AddToQueueSync(playlistItemContainer.GetChild(Random.Range(0, playlistItemContainer.childCount)).GetComponent<PlaylistItem>().PlayID);
            PlaylistItem playlistItem = playlistIndexer.GetRandomValidPlaylistItem();
            if (playlistItem == null)
            {
                Debug.LogWarning("[Playlist] No items in playlist. Cannot add random item to queue.");
                return;
            }
            AddToQueueSync(playlistItem.PlayID);
        }

        public void AddToQueueSync(int playID)
        {
            Debug.Log($"[Playlist] Add to queue: {playID}");

            if (playQueueItemContainer.childCount == 0 && !isPlayerBusy)
            {
                PlayByID(playID, Networking.LocalPlayer.displayName);
                return;
            }

            GameObject playlistItem = Instantiate(playQueueItemTemplate, playQueueItemContainer);
            int compactIdx = playIDCompactIdxMap[playID];
            if (compactIdx < 0)
            {
                Debug.LogError($"[Playlist] AddToQueueSync: playID {playID} is not in use.");
                playlistItem.GetComponent<PlayQueueItem>().Setup(this, playID, "Unknown Title", "Unknown Artist", "Unknown Genre", Networking.LocalPlayer.displayName);
                SyncQueue();
                return;
            }
            playlistItem.GetComponent<PlayQueueItem>().Setup(this, playID, playlistItemMetas[compactIdx].Title, playlistItemMetas[compactIdx].Artist, playlistItemMetas[compactIdx].Genre, Networking.LocalPlayer.displayName);

            SyncQueue();
        }

        public void RemoveFromQueueSync(PlayQueueItem playQueueItem)
        {
            Debug.Log($"[Playlist] Remove from queue: {playQueueItem.PlayID}");
            playQueueItem.gameObject.transform.SetParent(null);
            Destroy(playQueueItem.gameObject);

            SyncQueue();
        }

        public void MoveToTopQueueSync(PlayQueueItem playQueueItem)
        {
            Debug.Log($"[Playlist] Move to top queue: {playQueueItem.PlayID}");
            playQueueItem.transform.SetAsFirstSibling();

            SyncQueue();
        }

        private void SyncQueue()
        {
            Debug.Log("[Playlist] SyncQueue");
            if (!Networking.IsOwner(gameObject))
                Networking.SetOwner(Networking.LocalPlayer, gameObject);

            queueSize = playQueueItemContainer.childCount;
            for (int i = 0; i < queueSize; i++)
            {
                PlayQueueItem playQueueItem = playQueueItemContainer.GetChild(i).GetComponent<PlayQueueItem>();
                queuePlayIDs[i] = playQueueItem.PlayID;
                queuePlayUrls[i] = playQueueItem.PlayUrl;
                queueUsers[i] = playQueueItem.User;
            }

            RequestSerialization();
        }

        public override void OnDeserialization()
        {
            Debug.Log("[Playlist] OnDeserialization");

            if (isPlaylistLoaded)
            {
                RebuildQueueDisplay();
            }
        }

        private void RebuildQueueDisplay()
        {
            // ClearQueue
            foreach (Transform child in playQueueItemContainer.transform)
            {
                Destroy(child.gameObject);
            }

            // RebuildQueue
            for (int i = 0; i < queueSize; i++)
            {
                int playID = queuePlayIDs[i];
                VRCUrl playUrl = queuePlayUrls[i];
                string user = queueUsers[i];

                GameObject playlistItem = Instantiate(playQueueItemTemplate, playQueueItemContainer);
                if (playID < 0)
                {
                    playlistItem.GetComponent<PlayQueueItem>().Setup(this, playUrl, user);
                }
                else
                {
                    int compactIdx = playIDCompactIdxMap[playID];
                    if (compactIdx < 0)
                    {
                        Debug.LogError($"[Playlist] RebuildQueueDisplay: playID {playID} is not in use.");
                        playlistItem.GetComponent<PlayQueueItem>().Setup(this, playID, "Unknown Title", "Unknown Artist", "Unknown Genre", user);
                        continue;
                    }
                    playlistItem.GetComponent<PlayQueueItem>().Setup(this, playID, playlistItemMetas[compactIdx].Title, playlistItemMetas[compactIdx].Artist, playlistItemMetas[compactIdx].Genre, user);
                }
            }
        }
    }

}