#if UNITY_EDITOR

// #define USHARP_VIDEO_PLAYER
// #define VIZVID
// #define YAMASTREAM

using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BangumiPanel {
    
    public class BuildingPipeline : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        private PlaylistManager GetPlaylistController() {
            PlaylistManager[] playlistControllers = FindComponentGlobal<PlaylistManager>();
            if (playlistControllers == null)
            {
                return null;
            }

            if (playlistControllers.Length > 1)
            {
                throw new System.Exception($"[{GetType()}] More than one playlist controller found in scene. Please ensure only one exists.");
            }

            return playlistControllers[0];
        }

#if VIZVID
        private JLChnToZ.VRC.VVMW.Core BindVizVid(PlaylistManager playlistController)
        {

            JLChnToZ.VRC.VVMW.Core vizvidCore = playlistController.vizvidCore;

            if (vizvidCore == null)
            {
                vizvidCore = FindComponentGlobalFirst<JLChnToZ.VRC.VVMW.Core>();
                if (vizvidCore == null)
                {
                    return null;
                }
            }

            playlistController.vizvidCore = vizvidCore;

            return vizvidCore;
        }
#endif

#if YAMASTREAM
        private Yamadev.YamaStream.Controller BindYama(PlaylistManager playlistController)
        {

            Yamadev.YamaStream.Controller yamaController = playlistController.yamaController;

            if (yamaController == null)
            {
                yamaController = FindComponentGlobalFirst<Yamadev.YamaStream.Controller>();
                if (yamaController == null)
                {
                    return null;
                }
            }

            playlistController.yamaController = yamaController;

            return yamaController;
        }
#endif

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            Debug.Log($"[{GetType()}] Processing scene: " + scene.name);

            PlaylistManager playlistController = GetPlaylistController();
            if (playlistController == null) return;

#if VIZVID
            JLChnToZ.VRC.VVMW.Core playerCore = BindVizVid(playlistController);
#endif
#if YAMASTREAM
            Yamadev.YamaStream.Controller yamaController = BindYama(playlistController);
#endif
        }

        public T FindComponentGlobalFirst<T>() where T : Component
        {
            T[] components = FindComponentGlobal<T>();
            if (components == null)
            {
                return null;
            }
            return components[0];
        }

        public T[] FindComponentGlobal<T>() where T : Component
        {
            T[] components = Object.FindObjectsOfType<T>(true);
            if (components.Length == 0)
            {
                Debug.Log($"[{GetType()}] No {typeof(T).Name} found in scene.");
                return null;
            }

            Debug.Log($"[{GetType()}] Found {components.Length} {typeof(T).Name} in scene.");

            return components;
        }
    }

}

#endif
