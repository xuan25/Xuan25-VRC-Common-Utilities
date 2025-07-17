#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class VRCDebugClientLauncher : EditorWindow
{
    private static readonly string SETTING_GUID = "25858763eee551940842942d184ca9d7";

    static VRCDebugClientLauncherSetting vRCDebugClientLauncherSetting;

    public static VRCDebugClientLauncherSetting VRCDebugClientLauncherSetting
    {
        get
        {
            if (vRCDebugClientLauncherSetting == null)
            {
                vRCDebugClientLauncherSetting = AssetDatabase.LoadAssetAtPath<VRCDebugClientLauncherSetting>(AssetDatabase.GUIDToAssetPath(SETTING_GUID));
            }
            return vRCDebugClientLauncherSetting;
        }
    }

    public static string ClientInstallPath
    {
        get
        {
            if (!string.IsNullOrEmpty(VRCDebugClientLauncherSetting.ClientInstallPath))
            {
                return VRCDebugClientLauncherSetting.ClientInstallPath;
            }
            // Try to load from registry if not set in settings
            return VRC.Core.SDKClientUtilities.LoadRegistryVRCInstallPath();
        }
    }

    public static string generatedRoomId;

    public static string RoomID
    {
        get
        {
            if (!string.IsNullOrEmpty(VRCDebugClientLauncherSetting.RoomID))
                return VRCDebugClientLauncherSetting.RoomID;

            if (string.IsNullOrEmpty(generatedRoomId))
                generatedRoomId = VRC.Tools.GetRandomDigits(10);

            return generatedRoomId;
        }
    }

    public static string BuiltPath
    {
        get
        {
            if (!string.IsNullOrEmpty(VRCDebugClientLauncherSetting.BuildPath))
                return VRCDebugClientLauncherSetting.BuildPath;

            // Try to load from EditorPrefs if not set in settings
            return EditorPrefs.GetString("lastVRCPath", string.Empty);
        }
    }

    public static string BuiltPathParsed
    {
        get
        {
            if (!string.IsNullOrEmpty(VRCDebugClientLauncherSetting.BuildPathParsed))
                return VRCDebugClientLauncherSetting.BuildPathParsed;

            // Try to load from EditorPrefs if not set in settings
            return EditorPrefs.GetString("lastVRCPath", string.Empty);
        }
    }

    public static string BuiltUrl
    {
        get
        {
            if (string.IsNullOrEmpty(BuiltPathParsed))
            {
                return null;
            }

            string text = UnityWebRequest.EscapeURL(BuiltPathParsed).Replace("+", "%20");

            return string.Concat("vrchat://create?roomId=" + RoomID, "&url=file:///", text);

            // return VRC.SDKBase.Editor.VRC_SdkBuilder.GetLastUrl();
        }
    }

    public static string LaunchArgs
    {
        get
        {
            return string.IsNullOrEmpty(BuiltUrl) ? string.Empty : $"--url {BuiltUrl} {VRCDebugClientLauncherSetting.LaunchOptions}";
        }
    }


    // Add menu item named "My Window" to the Window menu
    [MenuItem("Window/VRCDebugClientLauncher")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(VRCDebugClientLauncher));
    }

    Vector2 scrollPos;

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        string clientInstallPath = EditorGUILayout.TextField("Client Install Path", ClientInstallPath);
        if (clientInstallPath != ClientInstallPath)
            VRCDebugClientLauncherSetting.ClientInstallPath = clientInstallPath;

        string builtPath = EditorGUILayout.TextField("Built Path", BuiltPath);
        if (builtPath != BuiltPath)
            VRCDebugClientLauncherSetting.BuildPath = builtPath;

        string roomID = EditorGUILayout.TextField("Room ID", RoomID);
        if (roomID != RoomID)
            VRCDebugClientLauncherSetting.RoomID = roomID;

        bool expandLaunchOptions = EditorGUILayout.Foldout(VRCDebugClientLauncherSetting.ExpandLaunchOptions, "Launch Options");
        if (expandLaunchOptions != VRCDebugClientLauncherSetting.ExpandLaunchOptions)
            VRCDebugClientLauncherSetting.ExpandLaunchOptions = expandLaunchOptions;

        if (VRCDebugClientLauncherSetting.ExpandLaunchOptions)
        {
            bool noVr = EditorGUILayout.Toggle("No VR", VRCDebugClientLauncherSetting.NoVr);
            if (noVr != VRCDebugClientLauncherSetting.NoVr)
                VRCDebugClientLauncherSetting.NoVr = noVr;

            string profile = EditorGUILayout.TextField("Profile", VRCDebugClientLauncherSetting.Profile);
            if (profile != VRCDebugClientLauncherSetting.Profile)
                VRCDebugClientLauncherSetting.Profile = profile;

            string fps = EditorGUILayout.TextField("FPS", VRCDebugClientLauncherSetting.Fps);
            if (fps != VRCDebugClientLauncherSetting.Fps)
                VRCDebugClientLauncherSetting.Fps = fps;

            bool enableDebugGui = EditorGUILayout.Toggle("Enable Debug GUI", VRCDebugClientLauncherSetting.EnableDebugGui);
            if (enableDebugGui != VRCDebugClientLauncherSetting.EnableDebugGui)
                VRCDebugClientLauncherSetting.EnableDebugGui = enableDebugGui;

            bool enableSdkLogLevels = EditorGUILayout.Toggle("Enable SDK Log Levels", VRCDebugClientLauncherSetting.EnableSdkLogLevels);
            if (enableSdkLogLevels != VRCDebugClientLauncherSetting.EnableSdkLogLevels)
                VRCDebugClientLauncherSetting.EnableSdkLogLevels = enableSdkLogLevels;

            bool enableUdonDebugLogging = EditorGUILayout.Toggle("Enable Udon Debug Logging", VRCDebugClientLauncherSetting.EnableUdonDebugLogging);
            if (enableUdonDebugLogging != VRCDebugClientLauncherSetting.EnableUdonDebugLogging)
                VRCDebugClientLauncherSetting.EnableUdonDebugLogging = enableUdonDebugLogging;

            bool skipRegistryInstall = EditorGUILayout.Toggle("Skip Registry Install", VRCDebugClientLauncherSetting.SkipRegistryInstall);
            if (skipRegistryInstall != VRCDebugClientLauncherSetting.SkipRegistryInstall)
                VRCDebugClientLauncherSetting.SkipRegistryInstall = skipRegistryInstall;

            string midi = EditorGUILayout.TextField("Midi", VRCDebugClientLauncherSetting.Midi);
            if (midi != VRCDebugClientLauncherSetting.Midi)
                VRCDebugClientLauncherSetting.Midi = midi;

            bool watchWorlds = EditorGUILayout.Toggle("Watch Worlds", VRCDebugClientLauncherSetting.WatchWorlds);
            if (watchWorlds != VRCDebugClientLauncherSetting.WatchWorlds)
                VRCDebugClientLauncherSetting.WatchWorlds = watchWorlds;

            bool watchAvatars = EditorGUILayout.Toggle("Watch Avatars", VRCDebugClientLauncherSetting.WatchAvatars);
            if (watchAvatars != VRCDebugClientLauncherSetting.WatchAvatars)
                VRCDebugClientLauncherSetting.WatchAvatars = watchAvatars;

            string ignoreTrackers = EditorGUILayout.TextField("Ignore Trackers", VRCDebugClientLauncherSetting.IgnoreTrackers);
            if (ignoreTrackers != VRCDebugClientLauncherSetting.IgnoreTrackers)
                VRCDebugClientLauncherSetting.IgnoreTrackers = ignoreTrackers;

            bool disableHwVideoDecoding = EditorGUILayout.Toggle("Disable HW Video Decoding", VRCDebugClientLauncherSetting.DisableHwVideoDecoding);
            if (disableHwVideoDecoding != VRCDebugClientLauncherSetting.DisableHwVideoDecoding)
                VRCDebugClientLauncherSetting.DisableHwVideoDecoding = disableHwVideoDecoding;

            bool enableHwVideoDecoding = EditorGUILayout.Toggle("Enable HW Video Decoding", VRCDebugClientLauncherSetting.EnableHwVideoDecoding);
            if (enableHwVideoDecoding != VRCDebugClientLauncherSetting.EnableHwVideoDecoding)
                VRCDebugClientLauncherSetting.EnableHwVideoDecoding = enableHwVideoDecoding;

            bool disableAmdStutterWorkaround = EditorGUILayout.Toggle("Disable AMD Stutter Workaround", VRCDebugClientLauncherSetting.DisableAmdStutterWorkaround);
            if (disableAmdStutterWorkaround != VRCDebugClientLauncherSetting.DisableAmdStutterWorkaround)
                VRCDebugClientLauncherSetting.DisableAmdStutterWorkaround = disableAmdStutterWorkaround;

            string osc = EditorGUILayout.TextField("OSC", VRCDebugClientLauncherSetting.Osc);
            if (osc != VRCDebugClientLauncherSetting.Osc)
                VRCDebugClientLauncherSetting.Osc = osc;

            string affinity = EditorGUILayout.TextField("Affinity", VRCDebugClientLauncherSetting.Affinity);
            if (affinity != VRCDebugClientLauncherSetting.Affinity)
                VRCDebugClientLauncherSetting.Affinity = affinity;

            string processPriority = EditorGUILayout.TextField("Process Priority", VRCDebugClientLauncherSetting.ProcessPriority);
            if (processPriority != VRCDebugClientLauncherSetting.ProcessPriority)
                VRCDebugClientLauncherSetting.ProcessPriority = processPriority;

            string mainThreadPriority = EditorGUILayout.TextField("Main Thread Priority", VRCDebugClientLauncherSetting.MainThreadPriority);
            if (mainThreadPriority != VRCDebugClientLauncherSetting.MainThreadPriority)
                VRCDebugClientLauncherSetting.MainThreadPriority = mainThreadPriority;

            string screenWidth = EditorGUILayout.TextField("Screen Width", VRCDebugClientLauncherSetting.ScreenWidth);
            if (screenWidth != VRCDebugClientLauncherSetting.ScreenWidth)
                VRCDebugClientLauncherSetting.ScreenWidth = screenWidth;

            string screenHeight = EditorGUILayout.TextField("Screen Height", VRCDebugClientLauncherSetting.ScreenHeight);
            if (screenHeight != VRCDebugClientLauncherSetting.ScreenHeight)
                VRCDebugClientLauncherSetting.ScreenHeight = screenHeight;

            string screenFullscreen = EditorGUILayout.TextField("Screen Fullscreen", VRCDebugClientLauncherSetting.ScreenFullscreen);
            if (screenFullscreen != VRCDebugClientLauncherSetting.ScreenFullscreen)
                VRCDebugClientLauncherSetting.ScreenFullscreen = screenFullscreen;

            string monitor = EditorGUILayout.TextField("Monitor", VRCDebugClientLauncherSetting.Monitor);
            if (monitor != VRCDebugClientLauncherSetting.Monitor)
                VRCDebugClientLauncherSetting.Monitor = monitor;

            string extraLaunchOptions = EditorGUILayout.TextField("Extra Launch Options", VRCDebugClientLauncherSetting.ExtraLaunchOptions);
            if (extraLaunchOptions != VRCDebugClientLauncherSetting.ExtraLaunchOptions)
                VRCDebugClientLauncherSetting.ExtraLaunchOptions = extraLaunchOptions;
        }

        EditorGUILayout.EndScrollView();

        GUIStyle style = new GUIStyle(EditorStyles.label);
        style.wordWrap = true;

        // EditorGUILayout.LabelField("Built Url", BuiltUrl, style);

        string launchArgs = LaunchArgs;
        EditorGUILayout.LabelField("Launch Arguments", launchArgs, style);

        if (GUILayout.Button("Launch VRChat Debugging Client", GUILayout.Height(40)))
        {
            System.Diagnostics.Process.Start(ClientInstallPath, launchArgs);
        }

    }
}
#endif
