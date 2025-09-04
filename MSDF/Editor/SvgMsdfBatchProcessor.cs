// Assets/Editor/SvgMsdfBatchProcessor.cs
// Unity Editor tool: Batch-process SVGs via Inkscape + msdfgen, preserving folder structure.
// Mirrors the provided Python script's features (scaling, stroke->path, optional union, modes, debug keeps).

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

public class SvgMsdfBatchProcessor : EditorWindow
{
    // -------- UI State --------
    [Serializable]
    public class Config : ScriptableObject
    {
        public string inputFolder = "";
        public string outputFolder = "";

        [Header("Executables (absolute paths)")]
        public string inkscapePath = "";     // e.g. C:\Program Files\Inkscape\bin\inkscape.exe or /usr/bin/inkscape
        public string msdfgenPath = "";      // e.g. C:\tools\msdfgen.exe or /usr/local/bin/msdfgen

        [Header("Optional XML Scale before Inkscape")]
        public bool enableXmlScale = false;
        public int scaleWidth = 64;
        public int scaleHeight = 64;

        public bool mergePaths = false;

        public Mode mode = Mode.msdf;

        [Header("msdfgen output dimensions")]
        public bool specifyOutputSize = false;
        public int outWidth = 64;
        public int outHeight = 64;

        
        [Header("Preprocessing & Debug")]
        public bool keepPreprocessed = false;
        public string preprocessedFolder = ""; // leave empty to place alongside outputs

        [Header("Logging")]
        public bool verbose = false;
    }

    public enum Mode { sdf, psdf, msdf, mtsdf }

    private Config cfg;

    // window fields
    private Vector2 _scroll;

    // footer sizing
    private const float FooterButtonHeight = 36f;
    private const float FooterPadding      = 6f;

    private GUIStyle _footerBg;
    private GUIStyle FooterBg {
        get {
            if (_footerBg == null) {
                _footerBg = new GUIStyle("ProjectBrowserBottomBarBg");
                if (_footerBg == null) _footerBg = new GUIStyle(EditorStyles.toolbar);
                _footerBg.padding = new RectOffset(0,0,0,0);
                _footerBg.fixedHeight = 0f;
            }
            return _footerBg;
        }
    }

    [MenuItem("Tools/Xuan25/SVG MSDF Batch Processor")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<SvgMsdfBatchProcessor>("SVG MSDF Batch Processor");
        wnd.minSize = new Vector2(520, 520);
    }

    void OnEnable()
    {
        if (cfg == null) cfg = CreateInstance<Config>();

        // If you persist cfg via EditorPrefs/Json, load here first, then:
        RefreshInputState();
        RefreshOutputState();
        RefreshExecState();
    }

    // ---- Validation cache (folders)
    bool _inputOK, _outputOK, _outputInsideAssets;
    string _inputMsg, _outputMsg;

    // ---- Executable cache (resolved on change)
    bool _inkOK, _msdfOK;
    string _inkMsg, _msdfMsg;
    string _inkResolvedPath, _msdfResolvedPath;

    private void UpdateExecCache(string toolName, string userPath, IEnumerable<string> hints,
                             out string resolvedPath, out bool ok, out string msg)
    {
        // 1) explicit path valid → use it
        if (!string.IsNullOrEmpty(userPath) && File.Exists(userPath))
        {
            resolvedPath = userPath;
            ok = true;
            msg = $"{toolName} path set explicitly.";
            return;
        }

        // 2) try PATH (FindInPath + ManualPathScan you already have)
        var found = ResolveExecutable(userPath, toolName, hints); // your helper from earlier
        if (!string.IsNullOrEmpty(found))
        {
            resolvedPath = found;
            ok = true;
            msg = $"{toolName} will be used from PATH: {found}";
            return;
        }

        resolvedPath = null;
        ok = false;
        msg = $"{toolName} not specified and not found on PATH.";
    }

    static bool SafeCanMakeFolder(string path)
    {
        try { Path.GetFullPath(path); return true; } catch { return false; }
    }

    void OnGUI()
    {
        // Scrollable content takes all remaining height
        using (new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(true)))
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));

            DrawPathSettingsFast();   // your validated fields with per-field help boxes
            DrawProcessingSettings(); // scaling → path merge → convert mode → output
            DrawDebugSettings();      // keep preprocessed, logs

            EditorGUILayout.Space(8);
            DrawUsageHelp();          // <-- back in (scrolls with content)

            EditorGUILayout.EndScrollView();
        }

        // Sticky footer stays visible
        DrawStickyFooter();
    }
    
    [SerializeField] private bool _isProcessing;

    private void DrawStickyFooter()
    {
        bool canProcess = !_isProcessing && _inputOK && _outputOK && _inkOK && _msdfOK;

        using (new GUILayout.HorizontalScope(FooterBg,
                GUILayout.ExpandWidth(true),
                GUILayout.Height(FooterButtonHeight + FooterPadding * 2)))
        {
            GUILayout.BeginVertical();
            GUILayout.Space(FooterPadding);

            using (new EditorGUI.DisabledScope(!canProcess))
            {
                if (GUILayout.Button(_isProcessing ? "Processing..." : "Process SVGs",
                                    GUILayout.Height(FooterButtonHeight)))
                {
                    try {
                        _isProcessing = true;
                        if (!string.IsNullOrEmpty(_inkResolvedPath))  cfg.inkscapePath = _inkResolvedPath;
                        if (!string.IsNullOrEmpty(_msdfResolvedPath)) cfg.msdfgenPath  = _msdfResolvedPath;
                        ValidateConfig(cfg);
                        ProcessAll(cfg);
                    }
                    catch (Exception ex) {
                        UnityEngine.Debug.LogError("[SVG MSDF] " + ex.Message);
                    }
                    finally {
                        _isProcessing = false;
                    }
                }
            }

            GUILayout.Space(FooterPadding);
            GUILayout.EndVertical();
        }
    }

    private void DrawPathSettingsFast()
    {
        using (new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.LabelField("Path Settings", EditorStyles.boldLabel);

            // Input folder
            if (DelayedPathField("Input Folder", ref cfg.inputFolder, isFolder: true))
                RefreshInputState();
            if (!_inputOK) EditorGUILayout.HelpBox(_inputMsg, MessageType.Error);

            // Output folder
            if (DelayedPathField("Output Folder", ref cfg.outputFolder, isFolder: true))
                RefreshOutputState();
            EditorGUILayout.HelpBox(_outputMsg, _outputOK ? MessageType.Info : MessageType.Error);

            // Inkscape
            if (DelayedPathField("Inkscape (optional)", ref cfg.inkscapePath, isFolder: false))
                RefreshExecState();
            EditorGUILayout.HelpBox(_inkMsg, _inkOK ? MessageType.Info : MessageType.Error);

            // msdfgen
            if (DelayedPathField("msdfgen (optional)", ref cfg.msdfgenPath, isFolder: false))
                RefreshExecState();
            EditorGUILayout.HelpBox(_msdfMsg, _msdfOK ? MessageType.Info : MessageType.Error);
        }
    }

    private void DrawProcessingSettings()
    {
        using (new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.LabelField("Processing Settings", EditorStyles.boldLabel);

            // Scaling settings
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Scaling", EditorStyles.miniBoldLabel);
                cfg.enableXmlScale = EditorGUILayout.Toggle("Enable XML scaling", cfg.enableXmlScale);
                using (new EditorGUI.DisabledScope(!cfg.enableXmlScale))
                {
                    cfg.scaleWidth = Mathf.Max(1, EditorGUILayout.IntField("Scale Width", cfg.scaleWidth));
                    cfg.scaleHeight = Mathf.Max(1, EditorGUILayout.IntField("Scale Height", cfg.scaleHeight));
                }
                EditorGUILayout.HelpBox("XML scaling adjusts width/height/viewBox and wraps content in a transformed <g>, preserving aspect ratio and centering before Inkscape runs.", MessageType.None);
            }

            // Path merging settings
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Path Merging", EditorStyles.miniBoldLabel);
                cfg.mergePaths = EditorGUILayout.Toggle(new GUIContent("Merge Paths (union)", "Union paths after stroke→path"), cfg.mergePaths);
            }

            // Convert mode settings
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Convert Mode", EditorStyles.miniBoldLabel);
                cfg.mode = (Mode)EditorGUILayout.EnumPopup("msdfgen Mode", cfg.mode);
                EditorGUILayout.HelpBox("sdf (true SDF), psdf (perpendicular), msdf (multi-channel, default), mtsdf (combined).", MessageType.None);
            }

            // Output settings
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Output", EditorStyles.miniBoldLabel);

                cfg.specifyOutputSize = EditorGUILayout.Toggle("Specify Output Size", cfg.specifyOutputSize);

                using (new EditorGUI.DisabledScope(!cfg.specifyOutputSize))
                {
                    cfg.outWidth  = Mathf.Max(1, EditorGUILayout.IntField("Width",  cfg.outWidth));
                    cfg.outHeight = Mathf.Max(1, EditorGUILayout.IntField("Height", cfg.outHeight));
                }

                if (!cfg.specifyOutputSize)
                    EditorGUILayout.HelpBox("No size specified: msdfgen will use its default raster size.", MessageType.Info);
            }
        }
    }

    private void DrawDebugSettings()
    {
        using (new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.LabelField("Debugging", EditorStyles.boldLabel);

            // Keep preprocessed files
            cfg.keepPreprocessed = EditorGUILayout.Toggle("Keep preprocessed SVGs", cfg.keepPreprocessed);
            using (new EditorGUI.DisabledScope(!cfg.keepPreprocessed))
            {
                cfg.preprocessedFolder = PathField("Preprocessed Folder (optional)", cfg.preprocessedFolder, false);
            }

            // Logs
            cfg.verbose = EditorGUILayout.Toggle("Verbose logs", cfg.verbose);
        }
    }

    // --- lightweight path state refresh ---
    
    private void RefreshInputState()
    {
        _inputOK = !string.IsNullOrEmpty(cfg.inputFolder) && Directory.Exists(cfg.inputFolder);
        _inputMsg = _inputOK ? "Input folder ready." : "Input must point to an existing folder.";
    }

    private void RefreshOutputState()
    {
        _outputOK = !string.IsNullOrEmpty(cfg.outputFolder) && SafeCanMakeFolder(cfg.outputFolder);
        _outputInsideAssets = _outputOK && IsInsideAssets(cfg.outputFolder);
        _outputMsg = !_outputOK
            ? "Output path will be created. Ensure it is a valid location."
            : (_outputInsideAssets ? "Auto-import enabled (inside Assets)." : "Outside Assets (files only).");
    }

    // executables: resolve (or tell user they’re coming from PATH)
    private void RefreshExecState()
    {
        UpdateExecCache("inkscape", cfg.inkscapePath, GetInkscapeHints(),
                        out _inkResolvedPath, out _inkOK, out _inkMsg);
        UpdateExecCache("msdfgen",  cfg.msdfgenPath,  GetMsdfgenHints(),
                        out _msdfResolvedPath, out _msdfOK, out _msdfMsg);
    }

    // -------- UI Helpers --------

    private static string PathField(string label, string path, bool folder)
    {
        EditorGUILayout.BeginHorizontal();
        path = EditorGUILayout.TextField(label, path);
        if (GUILayout.Button("...", GUILayout.Width(32)))
        {
            string picked = folder
                ? EditorUtility.OpenFolderPanel(label, string.IsNullOrEmpty(path) ? Application.dataPath : path, "")
                : EditorUtility.OpenFolderPanel(label, string.IsNullOrEmpty(path) ? Application.dataPath : Path.GetDirectoryName(path), "");
            if (!string.IsNullOrEmpty(picked)) path = picked;
        }
        EditorGUILayout.EndHorizontal();
        return path;
    }

    private static string FileField(string label, string path)
    {
        EditorGUILayout.BeginHorizontal();
        path = EditorGUILayout.TextField(label, path);
        if (GUILayout.Button("...", GUILayout.Width(32)))
        {
            string start = string.IsNullOrEmpty(path) ? Application.dataPath : Path.GetDirectoryName(path);
            string picked = EditorUtility.OpenFilePanel(label, start, "");
            if (!string.IsNullOrEmpty(picked)) path = picked;
        }
        EditorGUILayout.EndHorizontal();
        return path;
    }

    // PathField-like control that commits on Enter/focus-out,
    // AND updates immediately when you click the browse button.
    private static bool DelayedPathField(string label, ref string path, bool isFolder,
                                        string panelTitle = null, string fileExtensionFilter = "")
    {
        // Unique control name so we can manage focus reliably
        string ctrlName = "PF_" + label.GetHashCode();

        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginChangeCheck();
        GUI.SetNextControlName(ctrlName);
        string newPath = EditorGUILayout.DelayedTextField(label, path);
        bool changedByTyping = EditorGUI.EndChangeCheck();

        bool changedByButton = false;
        if (GUILayout.Button("...", GUILayout.Width(28)))
        {
            string start = string.IsNullOrEmpty(path)
                ? Application.dataPath
                : (isFolder ? path : Path.GetDirectoryName(path));

            string picked = isFolder
                ? EditorUtility.OpenFolderPanel(panelTitle ?? label, start, "")
                : EditorUtility.OpenFilePanel(panelTitle ?? label, start, fileExtensionFilter);

            if (!string.IsNullOrEmpty(picked))
            {
                newPath = picked;
                changedByButton = true;

                // Force any pending DelayedTextField edit to commit
                GUI.FocusControl(null);                 // break focus with the delayed field
                GUIUtility.keyboardControl = 0;         // ensure no text field remains active
                GUI.changed = true;                     // tell IMGUI something changed
                EditorWindow.focusedWindow?.Repaint();  // show new value right away
            }
        }

        EditorGUILayout.EndHorizontal();

        // sanitize + apply if changed
        newPath = SanitizePath(newPath);
        bool changed = changedByTyping || changedByButton;
        if (changed) path = newPath;
        return changed;
    }
    
    private void DrawUsageHelp()
    {
        EditorGUILayout.LabelField("Notes", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "• Requires Inkscape CLI and msdfgen in your system.\n" +
            "• The tool preserves input folder structure within the output folder.\n" +
            "• Inkscape actions used:\n" +
            "    select-all → object-stroke-to-path → select-all → (optional) path-union\n" +
            "• If XML scaling is enabled, SVG is scaled via XML before Inkscape.\n" +
            "• Preprocessed SVGs can be kept for debugging.",
            MessageType.Info);
    }

    // --- Validation helpers ---

    private static string SanitizePath(string p)
    {
        return string.IsNullOrEmpty(p) ? p : p.Trim().Trim('"').Replace('\\','/');
    }

    // Already in your script? If not, include it.
    private static string ToProjectRelativePath(string absolutePath)
    {
        if (string.IsNullOrEmpty(absolutePath)) return null;
        string abs = Path.GetFullPath(absolutePath).Replace('\\','/');
        string data = Path.GetFullPath(Application.dataPath).Replace('\\','/');
        if (abs.StartsWith(data))
            return "Assets" + abs.Substring(data.Length);
        return null; // outside Assets
    }

    private static bool IsInsideAssets(string path)
    {
        var rel = ToProjectRelativePath(path);
        return !string.IsNullOrEmpty(rel) && rel.StartsWith("Assets");
    }

    // --- PATH resolution ---

    private static string ResolveExecutable(string configuredPath, string toolName, IEnumerable<string> extraHints = null)
    {
        // 1) If user provided a valid path, use it
        if (!string.IsNullOrEmpty(configuredPath) && File.Exists(configuredPath))
            return configuredPath;

        // 2) Try OS 'where/which'
        var found = FindInPath(toolName);
        if (!string.IsNullOrEmpty(found))
            return found;

        // 3) Manual scan of PATH dirs
        found = ManualPathScan(toolName);
        if (!string.IsNullOrEmpty(found))
            return found;

        // 4) Check common install locations (hints)
        if (extraHints != null)
        {
            foreach (var h in extraHints)
            {
                if (!string.IsNullOrEmpty(h) && File.Exists(h))
                    return h;
            }
        }
        return null;
    }

    private static string FindInPath(string toolName)
    {
        try
        {
            bool isWindows = Application.platform == RuntimePlatform.WindowsEditor;
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = isWindows ? "where" : "which",
                Arguments = toolName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (var p = System.Diagnostics.Process.Start(psi))
            {
                string firstLine = p.StandardOutput.ReadLine();
                p.WaitForExit();
                if (!string.IsNullOrEmpty(firstLine))
                {
                    firstLine = firstLine.Trim().Trim('"');
                    if (File.Exists(firstLine)) return firstLine;
                }
            }
        }
        catch { /* ignore */ }
        return null;
    }

    private static string ManualPathScan(string toolName)
    {
        try
        {
            string pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
            var dirs = pathEnv.Split(Path.PathSeparator);
            string[] candidates = Application.platform == RuntimePlatform.WindowsEditor
                ? new[] { toolName + ".exe", toolName + ".cmd", toolName + ".bat", toolName + ".com", toolName }
                : new[] { toolName };

            foreach (var d in dirs)
            {
                if (string.IsNullOrEmpty(d)) continue;
                foreach (var c in candidates)
                {
                    var full = Path.Combine(d, c);
                    try { if (File.Exists(full)) return full; } catch { }
                }
            }
        }
        catch { }
        return null;
    }

    private static IEnumerable<string> GetInkscapeHints()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
            return new[]
            {
                @"C:\Program Files\Inkscape\bin\inkscape.exe",
                @"C:\Program Files\Inkscape\inkscape.com"
            };
        // macOS app bundle + common bins
        return new[]
        {
            "/Applications/Inkscape.app/Contents/MacOS/inkscape",
            "/opt/homebrew/bin/inkscape",
            "/usr/local/bin/inkscape",
            "/usr/bin/inkscape",
            "/snap/bin/inkscape"
        };
    }

    private static IEnumerable<string> GetMsdfgenHints()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
            return new[]
            {
                @"C:\Program Files\msdfgen\msdfgen.exe",
                @"C:\msdfgen\msdfgen.exe"
            };
        return new[]
        {
            "/opt/homebrew/bin/msdfgen",
            "/usr/local/bin/msdfgen",
            "/usr/bin/msdfgen"
        };
    }


    // -------- Processing --------

    private static void ValidateConfig(Config c)
    {
        if (string.IsNullOrEmpty(c.inputFolder) || !Directory.Exists(c.inputFolder))
            throw new Exception("Input folder does not exist.");
        if (string.IsNullOrEmpty(c.outputFolder))
            throw new Exception("Output folder is empty.");

        // Only enforce width/height if the user chose to specify them
        if (c.specifyOutputSize)
        {
            if (c.outWidth <= 0 || c.outHeight <= 0)
                throw new Exception("Output dimensions must be positive.");
        }

        if (c.enableXmlScale && (c.scaleWidth <= 0 || c.scaleHeight <= 0))
            throw new Exception("Scale dimensions must be positive.");

        // Resolve executables (your existing resolver)
        c.inkscapePath = ResolveExecutable(c.inkscapePath, "inkscape", GetInkscapeHints());
        c.msdfgenPath  = ResolveExecutable(c.msdfgenPath,  "msdfgen",  GetMsdfgenHints());

        if (string.IsNullOrEmpty(c.inkscapePath) || !File.Exists(c.inkscapePath))
            throw new Exception("Inkscape not found. Specify its path or add it to PATH.");
        if (string.IsNullOrEmpty(c.msdfgenPath) || !File.Exists(c.msdfgenPath))
            throw new Exception("msdfgen not found. Specify its path or add it to PATH.");
    }

    private static void ProcessAll(Config c)
    {
        var inputRoot = new DirectoryInfo(c.inputFolder);
        var svgs = inputRoot.Exists
            ? inputRoot.GetFiles("*.svg", SearchOption.AllDirectories)
            : Array.Empty<FileInfo>();

        if (svgs.Length == 0)
        {
            UnityEngine.Debug.Log("[SVG MSDF] No SVG files found in: " + c.inputFolder);
            return;
        }

        Directory.CreateDirectory(c.outputFolder);
        if (c.keepPreprocessed && !string.IsNullOrEmpty(c.preprocessedFolder))
            Directory.CreateDirectory(c.preprocessedFolder);

        int processed = 0;
        int failed = 0;

        var tempToDelete = new List<string>();
        var keptPreprocessed = new List<string>();

        try
        {
            for (int i = 0; i < svgs.Length; i++)
            {
                var f = svgs[i];
                string relPath = GetRelativePath(f.FullName, inputRoot.FullName);
                string relDir = Path.GetDirectoryName(relPath) ?? "";
                string nameNoExt = Path.GetFileNameWithoutExtension(relPath);

                string outDir = Path.Combine(c.outputFolder, relDir);
                Directory.CreateDirectory(outDir);

                string outputPng = Path.Combine(outDir, nameNoExt + ".png");

                bool cancel = EditorUtility.DisplayCancelableProgressBar(
                    "SVG → msdfgen",
                    $"[{i + 1}/{svgs.Length}] {relPath}",
                    (float)(i + 1) / svgs.Length
                );
                if (cancel) break;

                try
                {
                    if (c.verbose) UnityEngine.Debug.Log($"[SVG MSDF] Processing {relPath}");

                    string currentSvg = f.FullName;

                    // 1) Optional XML scaling
                    string xmlScaledPath = null;
                    if (c.enableXmlScale)
                    {
                        xmlScaledPath = CreateTempPath("xml_scaled_", ".svg");
                        XmlScaleSvg(f.FullName, xmlScaledPath, c.scaleWidth, c.scaleHeight, c.verbose);
                        currentSvg = xmlScaledPath;
                        tempToDelete.Add(xmlScaledPath);
                        if (c.verbose) UnityEngine.Debug.Log($"[SVG MSDF] XML scaled → {c.scaleWidth}x{c.scaleHeight}");
                    }

                    // 2) Inkscape stroke->path (+ optional union), exporting new SVG
                    string inkOutput = CreateTempPath("inkscape_processed_", ".svg");
                    bool inkOk = RunInkscape(c.inkscapePath, currentSvg, inkOutput, c.mergePaths, c.verbose);

                    string toFeedMsdf = inkOk && File.Exists(inkOutput) && new FileInfo(inkOutput).Length > 0
                        ? inkOutput
                        : currentSvg;

                    if (inkOk)
                    {
                        tempToDelete.Add(inkOutput);
                        if (c.keepPreprocessed)
                        {
                            string dst = ComputePreprocessedDestination(c, relDir, nameNoExt, ".preprocessed.svg");
                            Directory.CreateDirectory(Path.GetDirectoryName(dst) ?? c.outputFolder);
                            File.Copy(inkOutput, dst, true);
                            keptPreprocessed.Add(dst);
                            if (c.verbose) UnityEngine.Debug.Log($"[SVG MSDF] Saved preprocessed: {dst}");
                        }
                    }

                    // 3) Run msdfgen
                    bool ok = RunMsdfGen(c.msdfgenPath, toFeedMsdf, outputPng,
                        c.outWidth, c.outHeight, c.mode, c.verbose, c.specifyOutputSize);
                    if (ok)
                    {
                        processed++;
                        ImportIfInAssets(outputPng, c.verbose);
                        if (c.verbose) UnityEngine.Debug.Log($"[SVG MSDF] ✓ {outputPng}");
                    }
                    else
                    {
                        failed++;
                        UnityEngine.Debug.LogError($"[SVG MSDF] ✗ Failed: {relPath}");
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    UnityEngine.Debug.LogError($"[SVG MSDF] Exception processing {relPath}: {ex.Message}");
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();

            // Cleanup temps (unless they were kept)
            foreach (var t in tempToDelete)
            {
                try { if (File.Exists(t)) File.Delete(t); } catch { /* ignore */ }
            }

            UnityEngine.Debug.Log($"[SVG MSDF] Processing complete.\n  Success: {processed}\n  Failed: {failed}");
            if (c.keepPreprocessed && keptPreprocessed.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Kept {keptPreprocessed.Count} preprocessed SVG(s):");
                foreach (var p in keptPreprocessed) sb.AppendLine("  " + p);
                UnityEngine.Debug.Log(sb.ToString());
            }
        }
    }

    // -------- Inkscape + msdfgen --------

    private static bool RunInkscape(string inkscapePath, string inputSvg, string outputSvg, bool mergePaths, bool verbose)
    {
        // Build actions: select all, stroke->path, select all, optional union
        var actions = new List<string> { "select-all", "object-stroke-to-path", "select-all" };
        if (mergePaths) actions.Add("path-union");
        string actionsStr = string.Join(";", actions);

        var psi = new ProcessStartInfo
        {
            FileName = inkscapePath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        // Inkscape CLI (1.0+) supports --actions
        // inkscape input.svg --actions="select-all;object-stroke-to-path;select-all;path-union" --export-type=svg --export-filename=out.svg
        psi.ArgumentList.Add(inputSvg);
        psi.ArgumentList.Add($"--actions={actionsStr}");
        psi.ArgumentList.Add("--export-type=svg");
        psi.ArgumentList.Add($"--export-filename={outputSvg}");

        if (verbose) UnityEngine.Debug.Log($"[SVG MSDF] Inkscape: {inkscapePath} {string.Join(" ", psi.ArgumentList)}");

        try
        {
            using (var p = Process.Start(psi))
            {
                string stdout = p.StandardOutput.ReadToEnd();
                string stderr = p.StandardError.ReadToEnd();
                p.WaitForExit();
                if (p.ExitCode == 0 && File.Exists(outputSvg) && new FileInfo(outputSvg).Length > 0)
                {
                    return true;
                }
                if (verbose)
                {
                    UnityEngine.Debug.LogWarning($"[SVG MSDF] Inkscape nonzero exit or empty output.\nSTDOUT:\n{stdout}\nSTDERR:\n{stderr}");
                }
                return false;
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[SVG MSDF] Inkscape failed: {ex.Message}");
            return false;
        }
    }

    private static bool RunMsdfGen(string msdfgenPath, string inputSvg, string outputPng,
                                int width, int height, Mode mode, bool verbose, bool specifySize)
    {
        var psi = new ProcessStartInfo
        {
            FileName = msdfgenPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        psi.ArgumentList.Add(mode.ToString()); // sdf/psdf/msdf/mtsdf
        psi.ArgumentList.Add("-svg");
        psi.ArgumentList.Add(inputSvg);

        if (specifySize) // NEW
        {
            psi.ArgumentList.Add("-dimensions");
            psi.ArgumentList.Add(width.ToString());
            psi.ArgumentList.Add(height.ToString());
        }

        psi.ArgumentList.Add("-o");
        psi.ArgumentList.Add(outputPng);

        if (verbose) UnityEngine.Debug.Log($"[SVG MSDF] msdfgen: {msdfgenPath} {string.Join(" ", psi.ArgumentList)}");

        try
        {
            using (var p = Process.Start(psi))
            {
                string stdout = p.StandardOutput.ReadToEnd();
                string stderr = p.StandardError.ReadToEnd();
                p.WaitForExit();
                if (p.ExitCode == 0) return true;

                if (!string.IsNullOrWhiteSpace(stderr))
                    UnityEngine.Debug.LogError($"[SVG MSDF] msdfgen error: {stderr.Trim()}");
                else if (verbose && !string.IsNullOrWhiteSpace(stdout))
                    UnityEngine.Debug.LogWarning($"[SVG MSDF] msdfgen output: {stdout.Trim()}");

                return false;
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[SVG MSDF] msdfgen failed: {ex.Message}");
            return false;
        }
    }

    // -------- XML Scaling (ports Python logic) --------

    private static void XmlScaleSvg(string inputSvg, string outputSvg, int targetW, int targetH, bool verbose)
    {
        XDocument doc = XDocument.Load(inputSvg);
        var root = doc.Root ?? throw new Exception("SVG has no root element.");

        // Get original dimensions
        (double origW, double origH) = ParseSvgDimensions(root);

        if (origW <= 0 || origH <= 0)
            throw new Exception($"Invalid SVG dimensions parsed: {origW}x{origH}");

        double scaleX = targetW / origW;
        double scaleY = targetH / origH;
        double scale = Math.Min(scaleX, scaleY);

        double scaledW = origW * scale;
        double scaledH = origH * scale;
        double offsetX = (targetW - scaledW) / 2.0;
        double offsetY = (targetH - scaledH) / 2.0;

        // Ensure xmlns present
        if (root.Attribute("xmlns") == null)
            root.SetAttributeValue("xmlns", "http://www.w3.org/2000/svg");

        // Set root width/height and viewBox
        root.SetAttributeValue("width", targetW.ToString());
        root.SetAttributeValue("height", targetH.ToString());
        root.SetAttributeValue("viewBox", $"0 0 {targetW} {targetH}");

        // Move content into a <g> and apply transform
        var contentGroup = GetOrCreateFirstGroup(root);
        // prepend transform (translate + scale)
        string newTransform = $"translate({offsetX.ToString("0.######")},{offsetY.ToString("0.######")}) scale({scale.ToString("0.######")})";
        var existing = contentGroup.Attribute("transform")?.Value ?? "";
        contentGroup.SetAttributeValue("transform", string.IsNullOrEmpty(existing) ? newTransform : $"{newTransform} {existing}");

        // Save
        Directory.CreateDirectory(Path.GetDirectoryName(outputSvg) ?? Path.GetTempPath());
        using (var fs = File.CreateText(outputSvg))
        {
            fs.WriteLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            fs.Write(doc.ToString(SaveOptions.DisableFormatting));
        }

        // Basic validation: re-load and ensure <svg>
        var test = XDocument.Load(outputSvg);
        if (test.Root == null || !test.Root.Name.LocalName.ToLower().Contains("svg"))
            throw new Exception("Generated file does not contain a valid <svg> root.");

        if (verbose)
        {
            UnityEngine.Debug.Log($"[SVG MSDF] XML scaling: {origW}x{origH} → {targetW}x{targetH}, scale={scale:0.###}, offset=({offsetX:0.#},{offsetY:0.#})");
        }
    }

    private static (double w, double h) ParseSvgDimensions(XElement root)
    {
        // width/height attributes or derive from viewBox
        double w = ParseDimension((string)root.Attribute("width"));
        double h = ParseDimension((string)root.Attribute("height"));

        if (w <= 0 || h <= 0)
        {
            var vb = (string)root.Attribute("viewBox");
            if (!string.IsNullOrEmpty(vb))
            {
                var parts = vb.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 4)
                {
                    double.TryParse(parts[2], out w);
                    double.TryParse(parts[3], out h);
                }
            }
        }

        if (w <= 0) w = 100;
        if (h <= 0) h = 100;
        return (w, h);
    }

    private static double ParseDimension(string s)
    {
        if (string.IsNullOrEmpty(s)) return -1;
        var m = Regex.Match(s, @"[\d.]+");
        if (m.Success && double.TryParse(m.Value, out var v))
            return v;
        return -1;
    }

    private static XElement GetOrCreateFirstGroup(XElement root)
    {
        // Try to find an existing top-level <g> that is not metadata-y
        var metadata = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "metadata", "defs", "title", "desc" };
        var firstG = root.Elements().FirstOrDefault(e => e.Name.LocalName.Equals("g", StringComparison.OrdinalIgnoreCase));
        if (firstG != null) return firstG;

        // Create new <g> and move non-metadata children into it
        XNamespace ns = root.Name.Namespace;
        var g = new XElement(ns + "g");
        var toMove = root.Elements().Where(e => !metadata.Contains(e.Name.LocalName)).ToList();
        foreach (var e in toMove)
        {
            e.Remove();
            g.Add(e);
        }
        root.Add(g);
        return g;
    }

    // -------- Utils --------

    static void ApplySpriteImportSettings(string assetPath)
    {
        var ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (ti == null) return;

        // Settings that are NOT in TextureImporterSettings:
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.alphaIsTransparency = false;
        ti.mipmapEnabled = true;
        ti.sRGBTexture = false;
        ti.textureCompression = TextureImporterCompression.CompressedHQ;
        ti.crunchedCompression = true;

        // One pass via TextureImporterSettings for sprite-specific fields
        var s = new TextureImporterSettings();
        ti.ReadTextureSettings(s);

        s.spriteMeshType = SpriteMeshType.FullRect;
        s.spriteGenerateFallbackPhysicsShape = false;
        s.ApplyTextureType(TextureImporterType.Sprite);

        ti.SetTextureSettings(s);

        // If you need platform overrides, they’re separate objects:
        // var ps = new TextureImporterPlatformSettings {
        //     name = "Standalone", overridden = true, maxTextureSize = 2048, format = TextureImporterFormat.RGBA32
        // };
        // ti.SetPlatformTextureSettings(ps);

        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();
    }

    private static void ImportIfInAssets(string absolutePngPath, bool verbose)
    {
        string projectRel = ToProjectRelativePath(absolutePngPath);
        if (string.IsNullOrEmpty(projectRel)) return; // output is outside Assets → no import

        // Import into the project
        AssetDatabase.ImportAsset(projectRel, ImportAssetOptions.ForceSynchronousImport);
        ApplySpriteImportSettings(projectRel);

        // Force Sprite settings
        var ti = AssetImporter.GetAtPath(projectRel) as TextureImporter;
        if (ti != null)
        {
            bool changed = false;

            if (ti.textureType != TextureImporterType.Sprite) { ti.textureType = TextureImporterType.Sprite; changed = true; }
            if (ti.spriteImportMode != SpriteImportMode.Single) { ti.spriteImportMode = SpriteImportMode.Single; changed = true; }
            if (ti.sRGBTexture) { ti.sRGBTexture = false; changed = true; }
            if (ti.alphaIsTransparency) { ti.alphaIsTransparency = false; changed = true; }
            if (!ti.mipmapEnabled) { ti.mipmapEnabled = true; changed = true; }

            // Read/modify/apply importer settings
            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            ti.SetTextureSettings(settings);

            if (changed)
            {
                EditorUtility.SetDirty(ti);
                ti.SaveAndReimport();
            }
        }

        if (verbose) UnityEngine.Debug.Log($"[SVG MSDF] Imported as Sprite: {projectRel}");
    }

    private static string GetRelativePath(string filespec, string folder)
    {
        // Ensure directory style
        if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            folder += Path.DirectorySeparatorChar;

        Uri pathUri = new Uri(filespec);
        Uri folderUri = new Uri(folder);
        return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
    }

    private static string CreateTempPath(string prefix, string ext)
    {
        string file = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
        string target = Path.Combine(Path.GetTempPath(), prefix + file + ext);
        return target;
    }

    private static string ComputePreprocessedDestination(Config c, string relDir, string nameNoExt, string suffix)
    {
        string baseDir = string.IsNullOrEmpty(c.preprocessedFolder)
            ? c.outputFolder
            : c.preprocessedFolder;
        return Path.Combine(baseDir, relDir, nameNoExt + suffix);
    }
}
#endif
