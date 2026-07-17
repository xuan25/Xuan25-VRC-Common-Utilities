
using UdonSharp;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Common;

namespace Xuan25.AnnouncementDownloader
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AnnouncementTextDownloader : UdonSharpBehaviour
    {
        [SerializeField] private VRC.SDKBase.VRCUrl announcementEndpoint;

        [SerializeField] private TMPro.TextMeshProUGUI announcementText;

        void Start()
        {
            announcementText.text = "Loading...";
            VRC.SDK3.StringLoading.VRCStringDownloader.LoadUrl(announcementEndpoint, (VRC.Udon.Common.Interfaces.IUdonEventReceiver)this);
        }

        public override void OnStringLoadSuccess(VRC.SDK3.StringLoading.IVRCStringDownload result)
        {
            string resultAsUTF8 = result.Result;
            Debug.Log($"[{nameof(AnnouncementTextDownloader)}]: Successfully downloaded announcement: {resultAsUTF8}");
            
            if (!VRC.SDK3.Data.VRCJson.TryDeserializeFromJson(resultAsUTF8, out VRC.SDK3.Data.DataToken data))
            {
                Debug.Log($"[{nameof(AnnouncementTextDownloader)}]: Failed to Deserialize json {resultAsUTF8} - {data}");
                announcementText.text = "Failed to load announcement.";
                return;
            }

            if (data.DataDictionary == null)
            {
                Debug.Log($"[{nameof(AnnouncementTextDownloader)}]: Failed to Deserialize json {resultAsUTF8} - {data}");
                announcementText.text = "Failed to load announcement.";
                return;
            }

            if (!data.DataDictionary.TryGetValue("content", out VRC.SDK3.Data.DataToken contentToken))
            {
                Debug.Log($"[{nameof(AnnouncementTextDownloader)}]: Failed to find content in json {resultAsUTF8} - {data}");
                announcementText.text = "Failed to load announcement.";
                return;
            }

            if (contentToken.String == null)
            {
                Debug.Log($"[{nameof(AnnouncementTextDownloader)}]: Failed to find content in json {resultAsUTF8} - {data}");
                announcementText.text = "Failed to load announcement.";
                return;
            }

            announcementText.text = contentToken.String;
        }
    }
}