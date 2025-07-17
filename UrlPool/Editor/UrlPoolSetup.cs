#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

public class UrlPoolSetup : EditorWindow {
    public static UrlPool poolObj;
    public static string urlTemplate = "http://127.0.0.1:5000/url/{0}";
    public static int numUrls = 32768;

    [MenuItem("UrlPool/Setup")]
    public static void ShowWindow() => GetWindow<UrlPoolSetup>("UrlPool Setup");

    public static GUIContent poolObjContent = new GUIContent("PoolObj", "The GameObject that contains the UrlPool component.");
    public static GUIContent urlTemplateContent = new GUIContent("URL Template", "The template for the URLs to create. Use {0} as a placeholder for the URL ID.");
    public static GUIContent numUrlsContent = new GUIContent("Number of URLs", "The number of URLs to create.");

    public void OnGUI()
    {
        EditorGUIUtility.labelWidth = 90;

        // game object of the pool
        using (new GUILayout.HorizontalScope(GUI.skin.box))
            poolObj = (UrlPool)EditorGUILayout.ObjectField(poolObjContent, poolObj, typeof(UrlPool), true);

        // url template
        using (new GUILayout.HorizontalScope(GUI.skin.box))
            urlTemplate = EditorGUILayout.TextField(urlTemplateContent, urlTemplate);

        // number of urls
        using (new GUILayout.HorizontalScope(GUI.skin.box))
            numUrls = EditorGUILayout.IntField(numUrlsContent, numUrls);

        // create button
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create URLs"))
                Create();
        }
    }

    // create the urls
    private void Create()
    {
        if (poolObj == null)
        {
            Debug.LogError("Pool Object is null!");
            return;
        }

        if (numUrls <= 0)
        {
            Debug.LogError("Number of URLs must be greater than 0!");
            return;
        }

        // get UrlPool component
        UrlPool urlPool = poolObj.GetComponent<UrlPool>();
        
        int urlId = 0;
        // create urls
        urlPool.Urls = new VRCUrl[numUrls];
        for (int i = 0; i < numUrls; i++)
            urlPool.Urls[i] = new VRCUrl(string.Format(urlTemplate, urlId++));

        Debug.Log($"Created {numUrls} URLs under {poolObj.name}");
    }
}

#endif
