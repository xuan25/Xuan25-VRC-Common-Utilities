#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Playlist {
    
    public class PlaylistBuildingPipeline : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        private PlaylistController[] GetPlaylistControllers() {
            PlaylistController[] playlistControllers = FindComponentGlobal<PlaylistController>();
            if (playlistControllers == null)
            {
                Debug.LogWarning("[PlaylistBuildingPipeline] No playlist controller found in scene.");
                return null;
            }

            Debug.Log($"[PlaylistBuildingPipeline] Found {playlistControllers.Length} playlist controllers in scene.");

            return playlistControllers;
        }
        
#if VIZVID

        private JLChnToZ.VRC.VVMW.Core BindPlayerCore(PlaylistController playlistController)
        {

            JLChnToZ.VRC.VVMW.Core playerCore = playlistController.playerCore;

            if (playerCore == null)
            {
                playerCore = FindComponentGlobalFirst<JLChnToZ.VRC.VVMW.Core>();
                if (playerCore == null)
                {
                    Debug.LogError("[PlaylistBuildingPipeline] No player core found in scene.");
                    return null;
                }
            }

            playlistController.playerCore = playerCore;
            playerCore._AddListener(playlistController);

            return playerCore;
        }

#endif

        private Button BindAudioStateController(PlaylistController playlistController, AudioStateController audioStateController, string buttonName, System.Action eventAction)
        {
            Button button = playlistController.GetComponentsInChildren<Button>(true).FirstOrDefault(x => x.name == buttonName);
            if (button == null)
            {
                throw new System.Exception($"[PlaylistBuildingPipeline] No button found in scene with name {buttonName}. Please ensure only one exists.");
            }

            UnityEventTools.AddStringPersistentListener(button.onClick, UdonSharpEditorUtility.GetBackingUdonBehaviour(audioStateController).SendCustomEvent, eventAction.Method.Name);
            return button;
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            Debug.Log("[PlaylistBuildingPipeline] Processing scene: " + scene.name);

            PlaylistController[] playlistControllers = GetPlaylistControllers();
            if (playlistControllers == null) return;

            foreach (PlaylistController playlistController in playlistControllers)
            {
#if VIZVID
                playlistController.playerName = "AvProPlayer";

                JLChnToZ.VRC.VVMW.Core playerCore = BindPlayerCore(playlistController);
                if (playerCore == null) continue;
#endif

                AudioStateController audioStateController = playlistController.audioStateController;
                if (audioStateController == null)
                {
                    audioStateController = FindComponentGlobalFirst<AudioStateController>();
                    if (audioStateController == null)
                    {
                        Debug.LogError("[PlaylistBuildingPipeline] No audio state controller found in scene.");
                        continue;
                    }
                }

                BindAudioStateController(playlistController, audioStateController, "Surrounding", audioStateController.SurroundingSet);
                BindAudioStateController(playlistController, audioStateController, "Stereo", audioStateController.SurroundingReset);
            }
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
                Debug.LogWarning($"[PlaylistBuildingPipeline] No {typeof(T).Name} found in scene.");
                return null;
            }

            return components;
        }
    }

}

#endif
