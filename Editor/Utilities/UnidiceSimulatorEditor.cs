using System.IO;
using Unidice.Simulator.Unidice;
using UnityEditor;
using UnityEngine;

namespace Unidice.Simulator.Utilities
{
    [CustomEditor(typeof(UnidiceSimulator))]
    public class UnidiceSimulatorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Export images")) ExportImages();
            base.OnInspectorGUI();
        }

        public void ExportImages()
        {
            var simulator = (UnidiceSimulator)target;
            var images = simulator.Images.GetImages();
            var path = EditorUtility.SaveFilePanel("Export images", EditorApplication.applicationPath, "image", "png");
            
            var number = 0;
            var folder = Path.GetDirectoryName(path);
            var file = Path.GetFileNameWithoutExtension(path);

            foreach (var image in images)
            {
                var finalPath = $"{folder}{Path.DirectorySeparatorChar}{file} {++number:D3}.png";
                File.WriteAllBytes(finalPath, image.EncodeToPNG());
            }
        }
    }
}