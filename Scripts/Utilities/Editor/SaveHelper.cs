using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Unidice.Simulator.Editor.Utilities
{
    public static class SaveHelper
    {
        [MenuItem("File/Save Scene and Project &s", false)]
        public static void SaveProject()
        {
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("Saved everything!");
        }
    }
}
