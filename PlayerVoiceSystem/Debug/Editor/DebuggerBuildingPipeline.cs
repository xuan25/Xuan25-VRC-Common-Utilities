#if UNITY_EDITOR

using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayerVoiceSystem.Debugging {
    
    public class DebuggerBuildingPipeline : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        private DebuggingPanel[] GetDeguggingPanels() {
            DebuggingPanel[] debuggingPanels = FindComponentGlobal<DebuggingPanel>();
            if (debuggingPanels == null)
            {
                Debug.LogWarning("[DebuggerBuildingPipeline] No debugging panel found in scene.");
                return null;
            }

            Debug.Log($"[DebuggerBuildingPipeline] Found {debuggingPanels.Length} debugging panels in scene.");

            return debuggingPanels;
        }

        private PlayerVoiceRoomController BindController(DebuggingPanel debuggingPanel) {

            PlayerVoiceRoomController playerVoiceRoomController = debuggingPanel.playerVoiceRoomController;

            if (playerVoiceRoomController == null)
            {
                playerVoiceRoomController = FindComponentGlobalFirst<PlayerVoiceRoomController>();
                if (playerVoiceRoomController == null)
                {
                    Debug.LogError("[DebuggerBuildingPipeline] No PlayerVoiceRoomController found in scene.");
                    return null;
                }
            }
            
            debuggingPanel.playerVoiceRoomController = playerVoiceRoomController;
            playerVoiceRoomController._AddListener(debuggingPanel);

            return playerVoiceRoomController;
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            Debug.Log("[DebuggerBuildingPipeline] Processing scene: " + scene.name);

            DebuggingPanel[] debuggingPanels = GetDeguggingPanels();
            if (debuggingPanels == null) return;

            foreach (DebuggingPanel debuggingPanel in debuggingPanels)
            {
                PlayerVoiceRoomController playerVoiceRoomController = BindController(debuggingPanel);
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
                Debug.LogWarning($"[DebuggerBuildingPipeline] No {typeof(T).Name} found in scene.");
                return null;
            }

            return components;
        }
    }

}

#endif
