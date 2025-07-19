# Xuan25 Common Utilities for VRChat Worlds

Although referred to as "common utilities", this is essentially a collection (or dump) of frequently reused code snippets and components from my VRChat world-building workflow.

> ⚠️ Formal documentation is not guaranteed to always be present.

These utilities are meant to be reused across multiple worlds or projects. Expect a mix of polished scripts, half-finished experiments, and quick solutions that I found useful at some point.

### ⚠️ Please Note: `DynamicTMPFontAtlasClearer`

The `DynamicTMPFontAtlasClearer` module includes a build-time pipeline that **clears the dynamic TMP font atlas** before publishing your VRChat world.  

This is intended to **reduce the world’s download size**, especially for mobile platforms (such as Quest) where strict file size limits apply.

#### 🔍 Trade-offs

- ✅ **Pros:** Significantly reduces upload/download size.
- ⚠️ **Cons:** The font atlas must be regenerated at runtime after the world loads, which can cause **unexpected frame drops** when text is first displayed.

#### 🚫 How to Disable

There is currently **no built-in way to disable** this functionality.  
To prevent it from running, you must **delete the `DynamicTMPFontAtlasClearer` module** from the package manually.


### Setup Git for Unity and U# (For Windows)

Requirement: Python Runtime

```sh
git config filter.usharp.clean "python .gitscripts/filter_usharp.py"
git config merge.unityyamlmerge.name "Unity Smart Merge"
git config merge.unityyamlmerge.driver '"C:/Program Files/Unity/Hub/Editor/2022.3.22f1/Editor/Data/Tools/UnityYAMLMerge.exe" merge -p %O %A %B %A'

git config --get filter.usharp.clean
git config --get merge.unityyamlmerge.name
git config --get merge.unityyamlmerge.driver
```
