using Unidice.SDK.System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unidice.Simulator.Utilities
{

    public class EditorSimulatorSceneLoader
    {
        private static bool _simulatorSceneLoaded;

        [InitializeOnLoadMethod] 
        private static void Initialize()
        {
            EditorApplication.hierarchyChanged += CheckSimulatorSceneLoaded;
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
            CheckSimulatorSceneLoaded();
        }

        private static void CheckSimulatorSceneLoaded()
        {
            if (Application.isPlaying) return;
            if (!Object.FindObjectOfType<AppRaycasterTarget>()) return; // Only if we have a scene open that requires the SceneManager

            _simulatorSceneLoaded = false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                
                if (scene.name == "Simulator")
                {
                    _simulatorSceneLoaded = true;
                    return;
                }
            }
        }

        static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            if (_simulatorSceneLoaded) return;
            if (Application.isPlaying) return;
            if (instanceID >= 0) return; // scenes always have negative ID
            if (EditorUtility.InstanceIDToObject(instanceID)) return; // some instances also have negative ID, exclude them

            var rect = selectionRect;
            //rect.xMin += rect.width / 3f * 2f;
            rect.xMax -= 10;
            rect.xMin = rect.xMax - 100;
            if (GUI.Button(rect, "Load Simulator"))
            {
                var scene = EditorSceneManager.OpenScene("Assets/Unidice Simulator/Scenes/Simulator.unity", OpenSceneMode.Additive);
                EditorSceneManager.MoveSceneBefore(scene, SceneManager.GetSceneAt(0));
                _simulatorSceneLoaded = true;
            }
        }
    }
}