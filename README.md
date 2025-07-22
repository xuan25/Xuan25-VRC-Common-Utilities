# Xuan25 Common Utilities for VRChat Worlds

Although referred to as "common utilities", this is essentially a collection (or dump) of frequently reused code snippets and components from my VRChat world-building workflow.

> ⚠️ Formal documentation is not guaranteed to always be present.

These utilities are meant to be reused across multiple worlds or projects. Expect a mix of polished scripts, half-finished experiments, and quick solutions that I found useful at some point.

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
git config filter.usharp.clean "python .gitscripts/filter_usharp.py""
```

### 2. Configure Git for Unity (Optional)

You can optionally use Unity’s YAML merge tool to handle potential Git merge conflicts (see: [Unity Docs](https://docs.unity3d.com/2022.3/Documentation/Manual/SmartMerge.html)). This tool allows Git to:

> merge scene and prefab and prefab files in a semantically correct way.

<span style="color:red">**⚠ Warning: This tool only guarantees semantic correctness in YAML, not correctness of the actual merged content. You are still responsible for verifying the final result.**</span>

If you understand and require this feature, use the following Git configuration command:

```sh
git config merge.unityyamlmerge.driver '"C:/Program Files/Unity/Hub/Editor/2022.3.22f1/Editor/Data/Tools/UnityYAMLMerge.exe" merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED"'
```

Note: The path to `UnityYAMLMerge.exe` may vary depending on your Unity installation method, and should be adapted accordingly.
