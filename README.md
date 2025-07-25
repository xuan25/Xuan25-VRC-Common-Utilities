# Xuan25 Common Utilities for VRChat Worlds

Although referred to as "common utilities", this is essentially a collection (or dump) of frequently reused code snippets and components from my VRChat world-building workflow.

> ⚠️ Formal documentation is not guaranteed to always be present.

These utilities are meant to be reused across multiple worlds or projects. Expect a mix of polished scripts, half-finished experiments, and quick solutions that I found useful at some point.

## Modules

- **AudioSynthesizer**: Udon-programmable audio source with filters. Includes example implementations of an `Oscillator` and an `IIRFilter`. CPU-intensive due to the low efficiency of UdonVM.
- **BangumiPanel**: Displays a list of seasonal Bangumi to play. Supported players: VizVid, YamaPlayer.
- **BGMVolumeControl**: Fades out background music when a video starts playing. Implemented for YamaPlayer.
- **DigiClock**: A simple digital clock.
- **DynamicTMPFontAtlasClearer**: Automatically clears dynamic TextMeshPro font atlas on build to reduce download size.
- **Fonts**: Font assets.
- **JoinNotification**: Plays audio and/or animation when a player joins or leaves an instance.
- **KaraokePlaylist**: Karaoke playlist system.
- **LTCGIBuildTargetUtility**: Configures LTCGI screen/emitter for different build targets. Supports auto-activation/inactivation of components on build target change and auto-removal on build for certain targets.
- **OnBuildHooks**: Automatically inactivates or removes GameObjects on build.
- **PlayerObjectSystem**: Activates GameObjects based on certain groups of players in the instance or for specific players.
- **PlayerVoiceSystem**: Provides isolation zones and microphones for player voice.
- **RandomHintDisplay (-Examples)**: Displays a random hint from a pool, synced or local, with animations. Examples include Hint Generator and Truth or Dare.
- **RayCastUtilities**: Enables raycast detection for PC cursors and VR controllers. Includes implementation of ScrollView with VR joystick and mouse scroll control.
- **SceneAssembly**: Aligns multiple scene trees (e.g., scene variants) on build.
- **SymbolConfigurator**: Automatic configures compiler symbols for the Udon compiler. Supports VIZVID (VizVid Player) and YAMA_STREAM (Yama Player).
- **Toggle**: Toggles GameObjects, either synced or local.
- **UrlPool**: Pool for VRCURL with a batch generation window.
- **VideoPlayer-7.1**: VizVid-based video player with 7.1-ready surrounding audio sources.
- **VRCDebugClientLauncher**: Debug client launcher window with advanced configurable launch options.
- **VRCUrlTemplateSetter**: Automatically sets the URL prefix for `VRCUrlInput` on open.


## ⚠️ Please Note: `DynamicTMPFontAtlasClearer`

The `DynamicTMPFontAtlasClearer` module includes a build-time pipeline that **clears the dynamic TMP font atlas** before publishing your VRChat world.  

This is intended to **reduce the world’s download size**, especially for mobile platforms (such as Quest) where strict file size limits apply.

### 🔍 Trade-offs

- ✅ **Pros:** Significantly reduces upload/download size.
- ⚠️ **Cons:** The font atlas must be regenerated at runtime after the world loads, which can cause **unexpected frame drops** when text is first displayed.

### 🚫 How to Disable

There is currently **no built-in way to disable** this functionality.  
To prevent it from running, you must **delete the `DynamicTMPFontAtlasClearer` module** from the package manually.

## Development

### 1. Configure Git for U# (Windows)

The U# compiler makes extensive GUID changes to scene, prefab, and asset files to associate compiled outputs. This causes version control to track numerous irrelevant changes, making it hard to follow real updates. Therefore, we require these volatile changes not be included when committing code modifications. Use the following Git configuration commands with your chosen runtime to automatically filter out these changes and prevent accidental commits.

#### Option 1) Using Python runtime

```sh
git config filter.usharp.process "python .gitscripts/filter_usharp_process.py""
```

#### Renormalized the repository

**<span style="color:red">Note: Whenever the clean filter is changed, the repo should be renormalized. See：[Git - attributes Documentation](https://git-scm.com/docs/gitattributes#:~:text=Note:%20Whenever%20the%20clean%20filter%20is%20changed,%20the%20repo%20should%20be%20renormalized)</span>**

```sh
git add --renormalize .
```

### 2. Configure Git for Unity (Optional)

You can optionally use Unity’s YAML merge tool to handle potential Git merge conflicts (see: [Unity Docs](https://docs.unity3d.com/2022.3/Documentation/Manual/SmartMerge.html)). This tool allows Git to:

> merge scene and prefab and prefab files in a semantically correct way.

**<span style="color:red">⚠ Warning: This tool only guarantees semantic correctness in YAML, not correctness of the actual merged content. You are still responsible for verifying the final result.</span>**

If you understand and require this feature, use the following Git configuration command:

```sh
git config merge.unityyamlmerge.driver '"C:/Program Files/Unity/Hub/Editor/2022.3.22f1/Editor/Data/Tools/UnityYAMLMerge.exe" merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED"'
```

Note: The path to `UnityYAMLMerge.exe` may vary depending on your Unity installation method, and should be adapted accordingly.
