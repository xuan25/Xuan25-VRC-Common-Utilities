#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace Xuan25.UdonTelemetry
{
    public class UdonTelemetryEndpointSetup : EditorWindow {
        public static UdonTelemetryEndpoint endpointObj;

        public static string endpointPrefix = "http://127.0.0.1:3000";
        public static string endpointPath = "demo/telemetry";

        public enum DataType
        {
            Vec2I8,
        }

        public static DataType dataType = DataType.Vec2I8;

        [MenuItem("Tools/Xuan25/UdonTelemetry/Endpoint Setup")]
        public static void ShowWindow() => GetWindow<UdonTelemetryEndpointSetup>("Endpoint Setup");

        public static GUIContent endpointObjLabel = new GUIContent("Telemetry Endpoint", "The GameObject that contains the TelemetryEndpoint component.");
        public static GUIContent endpointPrefixLabel = new GUIContent("Endpoint Prefix", "The URL prefix for the telemetry endpoint.");
        public static GUIContent endpointPathLabel = new GUIContent("Endpoint Path", "The path for the telemetry endpoint.");
        public static GUIContent dataTypeLabel = new GUIContent("Data Type", "The type of data to be sent to the telemetry endpoint.");

        public void OnGUI()
        {
            EditorGUIUtility.labelWidth = 90;

            using (new GUILayout.HorizontalScope(GUI.skin.box))
                endpointObj = (UdonTelemetryEndpoint)EditorGUILayout.ObjectField(endpointObjLabel, endpointObj, typeof(UdonTelemetryEndpoint), true);

            using (new GUILayout.HorizontalScope(GUI.skin.box))
                endpointPrefix = EditorGUILayout.TextField(endpointPrefixLabel, endpointPrefix);

            using (new GUILayout.HorizontalScope(GUI.skin.box))
                endpointPath = EditorGUILayout.TextField(endpointPathLabel, endpointPath);

            using (new GUILayout.HorizontalScope(GUI.skin.box))
                dataType = (DataType)EditorGUILayout.EnumPopup(dataTypeLabel, dataType);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Setup", GUILayout.Width(100)))
                    Setup();
            }
        }

        private void Setup()
        {
            if (endpointObj == null)
            {
                Debug.LogError("Endpoint Object is null!");
                return;
            }

            var endpoint = endpointObj.GetComponent<UdonTelemetryEndpoint>();
            if (endpoint == null)
            {
                Debug.LogError("UdonTelemetryEndpoint component is missing!");
                return;
            }

            int bitDepth;
            string dataTypeDesc;

            switch (DataType.Vec2I8)
            {
                case DataType.Vec2I8:
                    bitDepth = 16;
                    dataTypeDesc = "vec2i8";
                    break;

                // default:
                //     Debug.LogError("Unsupported data type!");
                //     return;
            }

            int hexDigits = Mathf.CeilToInt(bitDepth / 4f);
            int maxValue = (1 << bitDepth) - 1;
            int totalCount = maxValue + 1;

            Undo.RecordObject(endpoint, "Endpoint Setup");

            try
            {
                EditorUtility.DisplayProgressBar(
                    "Setting up Endpoint URLs",
                    "Creating URLs...",
                    0f
                );

                endpoint.urls = new VRCUrl[totalCount];

                for (int i = 0; i < totalCount; i++)
                {
                    string hexString = GetHexString(i, hexDigits);

                    string url = $"{endpointPrefix.Trim('/')}/{endpointPath.Trim('/')}/{dataTypeDesc}/{hexString}";

                    endpoint.urls[i] = new VRCUrl(url);

                    if (i % 100 == 0 || i == totalCount - 1)
                    {
                        float progress = (float)(i + 1) / totalCount;
                        bool canceled = EditorUtility.DisplayCancelableProgressBar(
                            "Setting up Endpoint URLs",
                            $"Creating URLs... ({i + 1}/{totalCount})",
                            progress
                        );

                        if (canceled)
                        {
                            Debug.LogWarning("Endpoint setup canceled.");
                            break;
                        }
                    }
                }

                Debug.Log($"Created {endpoint.urls.Length} URLs under {endpointObj.name}");

                PrefabUtility.RecordPrefabInstancePropertyModifications(endpoint);
                EditorUtility.SetDirty(endpoint);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private string GetHexString(int value, int hexDigits)
        {
            string hexString = "";
            for (int i = 0; i < hexDigits; i++) {
                int hexDigitValue = (value >> (4 * (hexDigits - 1 - i))) & 0x0F;
                hexString += GetHexChar(hexDigitValue);
            }
            return hexString;
        }

        private char GetHexChar(int value)
        {
            if (value < 10) return (char)('0' + value);
            return (char)('A' + (value - 10));
        }
    }

}

#endif
