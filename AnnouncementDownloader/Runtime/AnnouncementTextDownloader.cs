
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

        [SerializeField] private string textIfLoading = "Loading...";
        [SerializeField] private string textIfFailedToLoad = "Failed to load announcement.";

        void Start()
        {
            announcementText.text = textIfLoading;
            Debug.Log($"[{nameof(AnnouncementTextDownloader)}]: Downloading announcement from {announcementEndpoint}...");
            VRC.SDK3.StringLoading.VRCStringDownloader.LoadUrl(announcementEndpoint, (VRC.Udon.Common.Interfaces.IUdonEventReceiver)this);
        }

        public override void OnStringLoadError(VRC.SDK3.StringLoading.IVRCStringDownload result)
        {
            Debug.Log($"[{nameof(AnnouncementTextDownloader)}]: Failed to download announcement: {result.Error}");
            announcementText.text = textIfFailedToLoad;
        }

        public override void OnStringLoadSuccess(VRC.SDK3.StringLoading.IVRCStringDownload result)
        {
            string resultAsUTF8 = result.Result;
            Debug.Log($"[{nameof(AnnouncementTextDownloader)}]: Successfully downloaded announcement: {resultAsUTF8}");

            // Try to load as JSON first
            // Expected format: { "content": "Your announcement text here" }
            if (LoadAsJson(resultAsUTF8))
            {
                return;
            }

            // If that fails, try to load as plain text
            if (LoadAsPlainText(resultAsUTF8))
            {
                return;
            }

            // If both fail, show an error message
            announcementText.text = textIfFailedToLoad;
        }

        private bool LoadAsJson(string resultAsUTF8)
        {
            if (!VRC.SDK3.Data.VRCJson.TryDeserializeFromJson(resultAsUTF8, out VRC.SDK3.Data.DataToken data))
            {
                Debug.Log($"[{nameof(AnnouncementTextDownloader)}]: Failed to Deserialize json {resultAsUTF8} - {data}");
                return false;
            }

            if (data.DataDictionary == null)
            {
                Debug.Log($"[{nameof(AnnouncementTextDownloader)}]: Failed to Deserialize json {resultAsUTF8} - {data}");
                return false;
            }

            if (!data.DataDictionary.TryGetValue("content", out VRC.SDK3.Data.DataToken contentToken))
            {
                Debug.Log($"[{nameof(AnnouncementTextDownloader)}]: Failed to find content in json {resultAsUTF8} - {data}");
                return false;
            }

            if (contentToken.String == null)
            {
                Debug.Log($"[{nameof(AnnouncementTextDownloader)}]: Failed to find content in json {resultAsUTF8} - {data}");
                return false;
            }

            announcementText.text = contentToken.String;
            return true;
        }

        private bool LoadAsPlainText(string resultAsUTF8)
        {
            announcementText.text = resultAsUTF8;
            return true;
        }
    }
}