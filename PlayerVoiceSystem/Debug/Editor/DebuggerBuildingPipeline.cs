#if UNITY_EDITOR

using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Xuan25.PlayerVoiceSystem.Debugging {
    
    public class DebuggerBuildingPipeline : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        private RoomDebuggingPanel[] GetRoomDebuggingPanels() {
            RoomDebuggingPanel[] debuggingPanels = FindComponentGlobal<RoomDebuggingPanel>();
            if (debuggingPanels == null)
            {
                Debug.LogWarning("[DebuggerBuildingPipeline] No debugging panel found in scene.");
                return null;
            }

            Debug.Log($"[DebuggerBuildingPipeline] Found {debuggingPanels.Length} debugging panels in scene.");

            return debuggingPanels;
        }
        
        private TriggerDebuggingPanel[] GetTriggerDebuggingPanels() {
            TriggerDebuggingPanel[] debuggingPanels = FindComponentGlobal<TriggerDebuggingPanel>();
            if (debuggingPanels == null)
            {
                Debug.LogWarning("[DebuggerBuildingPipeline] No trigger debugging panel found in scene.");
                return null;
            }

            Debug.Log($"[DebuggerBuildingPipeline] Found {debuggingPanels.Length} trigger debugging panels in scene.");

            return debuggingPanels;
        }

        private PlayerVoiceRoomController BindController(RoomDebuggingPanel debuggingPanel)
        {

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

        private PlayerVoiceTrigger BindController(TriggerDebuggingPanel debuggingPanel)
        {
            PlayerVoiceTrigger playerVoiceTrigger = debuggingPanel.playerVoiceTrigger;

            if (playerVoiceTrigger == null)
            {
                playerVoiceTrigger = FindComponentGlobalFirst<PlayerVoiceTrigger>();
                if (playerVoiceTrigger == null)
                {
                    Debug.LogError("[DebuggerBuildingPipeline] No PlayerVoiceTrigger found in scene.");
                    return null;
                }
            }

            debuggingPanel.playerVoiceTrigger = playerVoiceTrigger;
            playerVoiceTrigger._AddListener(debuggingPanel);

            return playerVoiceTrigger;
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            Debug.Log("[DebuggerBuildingPipeline] Processing scene: " + scene.name);

            RoomDebuggingPanel[] roomDebuggingPanels = GetRoomDebuggingPanels();
            if (roomDebuggingPanels == null) return;

            foreach (RoomDebuggingPanel roomDebuggingPanel in roomDebuggingPanels)
            {
                PlayerVoiceRoomController playerVoiceRoomController = BindController(roomDebuggingPanel);
            }

            TriggerDebuggingPanel[] triggerDebuggingPanels = GetTriggerDebuggingPanels();
            foreach (TriggerDebuggingPanel triggerDebuggingPanel in triggerDebuggingPanels)
            {
                PlayerVoiceTrigger playerVoiceTrigger = BindController(triggerDebuggingPanel);
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
