using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/VRCDebugClientLauncherSetting", order = 1)]
public class VRCDebugClientLauncherSetting : ScriptableObject
{
    [SerializeField]
    public string ClientInstallPath;

    [SerializeField]
    public string BuildPath;

    [System.Runtime.InteropServices.DllImport("shell32.dll")]
    static extern int SHGetKnownFolderPath( [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);

    string GetKnownFolderPath(Guid knownFolderId)
    {
        IntPtr pszPath = IntPtr.Zero;
        try
        {
            int hr = SHGetKnownFolderPath(knownFolderId, 0, IntPtr.Zero, out pszPath);
            if (hr >= 0)
                return System.Runtime.InteropServices.Marshal.PtrToStringAuto(pszPath);
            throw System.Runtime.InteropServices.Marshal.GetExceptionForHR(hr);
        }
        finally
        {
            if (pszPath != IntPtr.Zero)
                System.Runtime.InteropServices.Marshal.FreeCoTaskMem(pszPath);
        }
    }

    private string vrcDataPath;

    private string VRCDataPath
    {
        get
        {
            if (!string.IsNullOrEmpty(vrcDataPath))
            {
                return vrcDataPath;
            }
            Guid localLowId = new Guid("A520A1A4-1780-4FF6-BD18-167343C5AF16");
            string localLowPath = GetKnownFolderPath(localLowId);
            vrcDataPath = Path.Combine(localLowPath, "VRChat", "VRChat");
            return vrcDataPath;
        }
    }

    public string BuildPathParsed
    {
        get
        {
            if (string.IsNullOrEmpty(BuildPath))
            {
                return null;
            }
            return BuildPath.Replace("${VRCDataPath}", VRCDataPath);
        }
    }

    [SerializeField]
    public string RoomID;


    [SerializeField]
    public bool ExpandLaunchOptions = true;


    [SerializeField]
    public bool NoVr = true;

    [SerializeField]
    public string Profile;

    [SerializeField]
    public string Fps;

    [SerializeField]
    public bool EnableDebugGui = true;

    [SerializeField]
    public bool EnableSdkLogLevels = true;

    [SerializeField]
    public bool EnableUdonDebugLogging = true;

    [SerializeField]
    public bool SkipRegistryInstall = true;

    [SerializeField]
    public string Midi;

    [SerializeField]
    public bool WatchWorlds = true;

    [SerializeField]
    public bool WatchAvatars = false;

    [SerializeField]
    public string IgnoreTrackers;

    [SerializeField]
    public bool DisableHwVideoDecoding = false;

    [SerializeField]
    public bool EnableHwVideoDecoding = false;

    [SerializeField]
    public bool DisableAmdStutterWorkaround = false;

    [SerializeField]
    public string Osc;

    [SerializeField]
    public string Affinity;

    [SerializeField]
    public string ProcessPriority;

    [SerializeField]
    public string MainThreadPriority;

    [SerializeField]
    public string ScreenWidth;

    [SerializeField]
    public string ScreenHeight;

    [SerializeField]
    public string ScreenFullscreen;

    [SerializeField]
    public string Monitor;

    [SerializeField]
    public string ExtraLaunchOptions;

    public string LaunchOptions
    {
        get
        {
            return $"{(NoVr ? "--no-vr " : "")}" +
                   $"{(string.IsNullOrEmpty(Profile) ? "" : $"--profile {Profile} ")}" +
                   $"{(string.IsNullOrEmpty(Fps) ? "" : $"--fps {Fps} ")}" +
                   $"{(EnableDebugGui ? "--enable-debug-gui " : "")}" +
                   $"{(EnableSdkLogLevels ? "--enable-sdk-log-levels " : "")}" +
                   $"{(EnableUdonDebugLogging ? "--enable-sdk-log-levels " : "")}" +
                   $"{(SkipRegistryInstall ? "--skip-registry-install " : "")}" +
                   $"{(string.IsNullOrEmpty(Midi) ? "" : $"--midi {Midi} ")}" +
                   $"{(WatchWorlds ? "--watch-worlds " : "")}" +
                   $"{(WatchAvatars ? "--watch-avatars " : "")}" +
                   $"{(string.IsNullOrEmpty(IgnoreTrackers) ? "" : $"--ignore-trackers {IgnoreTrackers} ")}" +
                   $"{(DisableHwVideoDecoding ? "--disable-hw-video-decoding " : "")}" +
                   $"{(EnableHwVideoDecoding ? "--enable-hw-video-decoding " : "")}" +
                   $"{(DisableAmdStutterWorkaround ? "--disable-amd-stutter-workaround " : "")}" +
                   $"{(string.IsNullOrEmpty(Osc) ? "" : $"--osc {Osc} ")}" +
                   $"{(string.IsNullOrEmpty(Affinity) ? "" : $"--affinity {Affinity} ")}" +
                   $"{(string.IsNullOrEmpty(ProcessPriority) ? "" : $"--process-priority {ProcessPriority} ")}" +
                   $"{(string.IsNullOrEmpty(MainThreadPriority) ? "" : $"--main-thread-priority {MainThreadPriority} ")}" +
                   $"{(string.IsNullOrEmpty(ScreenWidth) ? "" : $"-screen-width {ScreenWidth} ")}" +
                   $"{(string.IsNullOrEmpty(ScreenHeight) ? "" : $"-screen-height {ScreenHeight} ")}" +
                   $"{(string.IsNullOrEmpty(ScreenFullscreen) ? "" : $"-screen-fullscreen {ScreenFullscreen} ")}" +
                   $"{(string.IsNullOrEmpty(Monitor) ? "" : $"-monitor {Monitor} ")}" +
                   $"{(string.IsNullOrEmpty(ExtraLaunchOptions) ? "" : ExtraLaunchOptions + " ")}";
        }
    }
}
