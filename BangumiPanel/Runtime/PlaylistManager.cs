// #define USHARP_VIDEO_PLAYER
// #define VIZVID
// #define YAMASTREAM

using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace BangumiPanel
{
    public class PlaylistManager : UdonSharpBehaviour
    {

        public VRCUrl mainUrl;

#if USHARP_VIDEO_PLAYER
        public USharpVideoPlayer videoPlayer;
#endif
#if VIZVID
        public JLChnToZ.VRC.VVMW.Core vizvidCore;
        public string videoPlayerName = "AvProPlayer";
        private byte videoPlayerType = 0;
#endif
#if YAMASTREAM
        public Yamadev.YamaStream.Controller yamaController;
#endif

        public Transform bangumiListContainer;
        public Transform sourceListContainer;
        public Transform epListContainer;
        public GameObject buttonPrefab;
        public UrlPool urlPool;

        public TextMeshProUGUI lastUpdateText;

        private DataList bangumis;
        private int selectedBangumiIndex = -1;
        private int selectedSourceIndex = -1;
        private int selectedEpIndex = -1;


        void Start()
        {
#if VIZVID
            for (byte i = 0; i < vizvidCore.PlayerNames.Length; i++)
            {
                if (vizvidCore.PlayerNames[i] == videoPlayerName)
                {
                    videoPlayerType = (byte)(i + 1);
                    Debug.Log($"[Playlist] Player type: {videoPlayerType}");
                    break;
                }
            }
            if (videoPlayerType == 0)
            {
                Debug.LogError($"[Playlist] Player type not found: {videoPlayerName}");
            }
#endif
        }

        public void OnEnable()
        {
            Reload();
        }

        public void Reload()
        {
            VRCStringDownloader.LoadUrl(mainUrl, (IUdonEventReceiver)this);

            if (lastUpdateText != null)
                lastUpdateText.text = "Loading...";
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            string resultAsUTF8 = result.Result;
            VRCJson.TryDeserializeFromJson(resultAsUTF8, out DataToken dataToken);

            bangumis = dataToken.DataDictionary["bangumis"].DataList;

            foreach (Transform child in bangumiListContainer)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in sourceListContainer)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in epListContainer)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < bangumis.Count; i++)
            {
                GameObject button = Instantiate(buttonPrefab, bangumiListContainer);
                button.GetComponentInChildren<PlaylistButton>().Setup(this, bangumis[i].DataDictionary["title"].String, "bangumi", i);
            }

            if (lastUpdateText != null)
            {
                if (dataToken.DataDictionary.ContainsKey("last_update_timestamp"))
                {
                    // dataToken.DataDictionary.TryGetValue("last_update_timestamp", out DataToken lastUpdateToken);
                    // Debug.Log(lastUpdateToken.TokenType);
                    // double lastUpdateTimestamp = lastUpdateToken.Double;
                    double lastUpdateTimestamp = dataToken.DataDictionary["last_update_timestamp"].Double;
                    string lastUpdate = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(lastUpdateTimestamp).ToLocalTime().ToString("F");
                    lastUpdateText.text = lastUpdate;
                }
                else
                {
                    lastUpdateText.text = "N/A";
                }
            }
        }

        public void SelectBangumi(int index)
        {
            selectedBangumiIndex = index;
            OnSelectBangumi();
        }

        public void OnSelectBangumi()
        {
            selectedSourceIndex = -1;
            foreach (Transform child in sourceListContainer)
            {
                Destroy(child.gameObject);
            }

            selectedEpIndex = -1;
            foreach (Transform child in epListContainer)
            {
                Destroy(child.gameObject);
            }

            DataList sources = bangumis[selectedBangumiIndex].DataDictionary["stream_source"].DataList;
            for (int i = 0; i < sources.Count; i++)
            {
                GameObject button = Instantiate(buttonPrefab, sourceListContainer);
                button.GetComponentInChildren<PlaylistButton>().Setup(this, sources[i].DataDictionary["name"].String, "source", i);
            }
        }

        public void SelectSource(int index)
        {
            selectedSourceIndex = index;
            OnSelectSource();
        }

        public void OnSelectSource()
        {
            selectedEpIndex = -1;
            foreach (Transform child in epListContainer)
            {
                Destroy(child.gameObject);
            }

            DataList episodes = bangumis[selectedBangumiIndex].DataDictionary["stream_source"].DataList[selectedSourceIndex].DataDictionary["episodes"].DataList;

            for (int i = 0; i < episodes.Count; i++)
            {
                GameObject button = Instantiate(buttonPrefab, epListContainer);
                button.GetComponentInChildren<PlaylistButton>().Setup(this, episodes[i].DataDictionary["name"].String, "ep", i);
            }
        }

        public void SelectEp(int index)
        {
            selectedEpIndex = index;
            OnSelectEp();
        }

        public void OnSelectEp()
        {
            string bangumiName = bangumis[selectedBangumiIndex].DataDictionary["title"].String;
            string sourceName = bangumis[selectedBangumiIndex].DataDictionary["stream_source"].DataList[selectedSourceIndex].DataDictionary["name"].String;
            string epName = bangumis[selectedBangumiIndex].DataDictionary["stream_source"].DataList[selectedSourceIndex].DataDictionary["episodes"].DataList[selectedEpIndex].DataDictionary["name"].String;
            int urlId = (int)bangumis[selectedBangumiIndex].DataDictionary["stream_source"].DataList[selectedSourceIndex].DataDictionary["episodes"].DataList[selectedEpIndex].DataDictionary["url_proxy"].Double;
            VRCUrl url = urlPool.Urls[urlId];

#if USHARP_VIDEO_PLAYER
            videoPlayer.PlayVideo(url);
#endif
#if VIZVID
            videoPlayerCore.PlayUrl(url, videoPlayerType);
#endif
#if YAMASTREAM
            yamaController.TakeOwnership();
            yamaController.PlayTrack(Yamadev.YamaStream.Track.New(Yamadev.YamaStream.VideoPlayerType.AVProVideoPlayer, $"{bangumiName} - {epName} ({sourceName})", url));
#endif
        }

        public void UpdateSelection(string role, int index)
        {
            switch (role)
            {
                case "bangumi":
                    Debug.Log("Selecting bangumi");
                    SelectBangumi(index);
                    break;
                case "source":
                    Debug.Log("Selecting source");
                    SelectSource(index);
                    break;
                case "ep":
                    Debug.Log("Selecting ep");
                    SelectEp(index);
                    break;
                default:
                    Debug.Log("Unknown role");
                    break;
            }
        }
    }

}