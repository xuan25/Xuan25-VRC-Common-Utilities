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
        public VideoPlayerSyncHandler videoPlayerSyncHandler;
        #endif
        public PlaylistIndexer playlistIndexer;
        public PlaylistHotReload playlistHotReload;

        protected PlaylistItemMeta[] playlistItemMetas;

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

        void Start()
        {
#if UNITY_EDITOR
#if VIZVID
            playerName = "BuiltInPlayer";
            Debug.Log($"[Playlist] Player name overrides to {playerName} within editor."); 
#endif
#endif
#if VIZVID
            videoPlayerSyncHandler.Setup(this);
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
            if (VRCJson.TryDeserializeFromJson(json, out DataToken result))
            {
                int count = (int)result.DataDictionary["COUNT"].Double;
                playlistItemMetas = new PlaylistItemMeta[count];
                for (int i = 0; i < count; i++)
                {
                    GameObject playlistItemMeta = Instantiate(playlistItemMetaTemplate, playlistItemMetaContainer);
                    playlistItemMeta.GetComponentInChildren<PlaylistItemMeta>().Setup(this, i,
                        result.DataDictionary["TITLE"].DataList[i].String,
                        result.DataDictionary["ARTIST"].DataList[i].String,
                        result.DataDictionary["TITLE_ACRONYM"].DataList[i].String,
                        result.DataDictionary["ARTIST_ACRONYM"].DataList[i].String,
                        result.DataDictionary["GENRE"].DataList[i].String
                    );

                    playlistItemMetas[i] = playlistItemMeta.GetComponentInChildren<PlaylistItemMeta>();
                }

                int version = (int)result.DataDictionary["VERSION"].Double;
                playlistHotReload.SetLocalVersion(version);

                Debug.Log($"[Playlist] Loaded {count} playlist items");
            }
            else
            {
                Debug.Log($"[Playlist] Failed to Deserialize json {json} - {result}");
            }
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

        private void PlayByID(int playID)
        {
#if VIZVID
            playerCore.PlayUrl(urlPool.Urls[playID], playerType);
            videoPlayerSyncHandler.SetMetadata(playlistItemMetas[playID].Title, playlistItemMetas[playID].Artist);
#endif

            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(OnPlayScheduled));
            isPlayerBusy = true;
        }

        private void PlayByUrl(VRCUrl playUrl)
        {
#if VIZVID
            playerCore.PlayUrl(playUrl, playerType);
            videoPlayerSyncHandler.SetMetadata(string.Empty, string.Empty);
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
                PlayByUrl(playQueueItem.PlayUrl);
            }
            else
            {
                PlayByID(playQueueItem.PlayID);
            }
            RemoveFromQueueSync(playQueueItem);
        }

        public void AddToQueueSync(VRCUrl playUrl)
        {
            Debug.Log($"[Playlist] Add to queue: {playUrl}");

            if (playQueueItemContainer.childCount == 0 && !isPlayerBusy)
            {
                PlayByUrl(playUrl);
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
                PlayByID(playID);
                return;
            }

            GameObject playlistItem = Instantiate(playQueueItemTemplate, playQueueItemContainer);
            playlistItem.GetComponent<PlayQueueItem>().Setup(this, playID, playlistItemMetas[playID].Title, playlistItemMetas[playID].Artist, playlistItemMetas[playID].Genre, Networking.LocalPlayer.displayName);

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
                    playlistItem.GetComponent<PlayQueueItem>().Setup(this, playID, playlistItemMetas[playID].Title, playlistItemMetas[playID].Artist, playlistItemMetas[playID].Genre, user);
                }
            }
        }
    }

}