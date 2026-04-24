
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.UdonTelemetry
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UdonTelemetryPlayerPosition : UdonSharpBehaviour
    {
        [SerializeField] private UdonTelemetryEndpoint telemetryEndpoint;

        // "radius" of the telemetry area in meters, centered on the GameObject's position
        [SerializeField] private float xRangeMeter = 10;
        // [SerializeField] private float yRangeMeter = 10;
        [SerializeField] private float zRangeMeter = 10;

        [SerializeField] private float reportIntervalSeconds = 120f;

        [SerializeField] private bool verbose = false;

        const int ENCODE_BIT = 8;

        private bool sessionActive = false;

        void OnEnable()
        {
            BeginTelemetrySession();
        }

        void OnDisable()
        {
            EndTelemetrySession();
        }

        public void BeginTelemetrySession()
        {
            sessionActive = true;
            SendCustomEventDelayedSeconds(nameof(TelemetryLoop), reportIntervalSeconds);
            Debug.Log($"[{nameof(UdonTelemetry)}] Telemetry session started.");
        }

        public void EndTelemetrySession()
        {
            sessionActive = false;
            Debug.Log($"[{nameof(UdonTelemetry)}] Telemetry session ended.");
        }

        public void TelemetryLoop()
        {
            if (!sessionActive) return;
            SendCustomEventDelayedSeconds(nameof(TelemetryLoop), reportIntervalSeconds);

            ReportTelemetryData();
        }

#if UNITY_EDITOR

        private char GetHexChar(int value)
        {
            if (value < 10) return (char)('0' + value);
            return (char)('A' + (value - 10));
        }

#endif

        private void ReportTelemetryData()
        {
            if (telemetryEndpoint == null)
            {
                Debug.LogError("Telemetry Endpoint is not set!");
                return;
            }

            // player position relative to the anchor as (0, 0, 0) in world units (meters)
            Vector3 positionReal = gameObject.transform.InverseTransformPoint(Networking.LocalPlayer.GetPosition());

            // compute normalized position [-1, 1] with respect to the telemetry range
            // e.g. if xRangeMeter = 10, then x = -10 maps to -1, x = 0 maps to 0, x = 10 maps to 1
            Vector3 range = new Vector3(1 / xRangeMeter, 1, 1 / zRangeMeter);
            Vector3 positionNormalized = Vector3.Scale(positionReal, range);

            // early return if out of range
            if (Mathf.Abs(positionNormalized.x) > 1 || Mathf.Abs(positionNormalized.y) > 1 || Mathf.Abs(positionNormalized.z) > 1)
            {
#if UNITY_EDITOR
                if (verbose) Debug.Log($"[{nameof(UdonTelemetry)}] Player is out of telemetry range.");
#endif
                return;
            }

            // encoded into i8 [-1, 1] -> [-128, 127] range
            // transform the normalized position into encoding space (i8)
            // e.g. if x = 0.5, then xI8 = 64; if x = -1, then xI8 = -128; if x = 1, then xI8 = 127
            Vector3 positionEncoded = positionNormalized * sbyte.MaxValue;

#if UNITY_EDITOR

            // encode into hex string, e.g. "7F7F" for (127, 127), "8000" for (-128, 0), "00FF" for (0, -1)
            char[] hexChars = new char[4];
            for (int i = 0; i < 2; i++)
            {
                int value = (i == 0) ? (int)positionEncoded.x : (int)positionEncoded.z;
                if (value < 0) value += 256; // convert to unsigned byte
                hexChars[i * 2] = GetHexChar(value >> 4);
                hexChars[i * 2 + 1] = GetHexChar(value & 0x0F);
            }
            string hexString = new string(hexChars);

            if (verbose) Debug.Log($"[{nameof(UdonTelemetry)}] Reporting telemetry data: {hexString} (normalized: {positionNormalized}, relative: {positionReal})");

#endif

            int xIndex = (int)positionEncoded.x;
            int zIndex = (int)positionEncoded.z;

            if (xIndex < 0) xIndex += byte.MaxValue + 1; // convert from signed byte to unsigned byte
            if (zIndex < 0) zIndex += byte.MaxValue + 1; // convert from signed byte to unsigned byte

            int urlId = (xIndex << ENCODE_BIT) | zIndex; // combine x and z into a single index
            VRCUrl url = telemetryEndpoint.GetUrl(urlId);

#if UNITY_EDITOR
            if (verbose) Debug.Log($"[{nameof(UdonTelemetry)}] Sending telemetry data to URL: {url}");
#endif

            VRC.SDK3.StringLoading.VRCStringDownloader.LoadUrl(url);
        }

        public override void OnStringLoadSuccess(VRC.SDK3.StringLoading.IVRCStringDownload result)
        {
#if UNITY_EDITOR
            if (verbose) Debug.Log($"Telemetry data sent successfully.");
#endif
        }

        public override void OnStringLoadError(VRC.SDK3.StringLoading.IVRCStringDownload result)
        {
#if UNITY_EDITOR
            if (verbose) Debug.Log($"Telemetry data failed to send.");
#endif
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            // Convert the local coordinate values into world
            // coordinates for the matrix transformation.
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.color = new Color(0.75f, 0.0f, 0.25f, 0.75f);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(xRangeMeter * 2, 0.1f, zRangeMeter * 2));
            Gizmos.color = new Color(0.75f, 0.0f, 0.25f, 0.25f);
            Gizmos.DrawCube(Vector3.zero, new Vector3(xRangeMeter * 2, 0.1f, zRangeMeter * 2));

            // Draw sample points

            float samplingResolution = 1 << ENCODE_BIT; // e.g. 256 for 8 bits encoding
            float xStep = xRangeMeter * 2 / samplingResolution;
            float zStep = zRangeMeter * 2 / samplingResolution;

            // Draw sampling resolution

            Vector3 rayNormal = Vector3.up * 0.1f * 0.5f;
            for (int x = 0; x <= samplingResolution; x++)
            {
                for (int z = 0; z <= samplingResolution; z++)
                {
                    Vector3 samplePos = new Vector3(-xRangeMeter + x * xStep, 0, -zRangeMeter + z * zStep);
                    Gizmos.DrawLine(samplePos + rayNormal, samplePos - rayNormal);
                }   
            }
            
        }
#endif

    }
}