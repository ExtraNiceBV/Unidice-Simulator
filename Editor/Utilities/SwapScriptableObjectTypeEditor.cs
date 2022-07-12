using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unidice.Simulator.Utilities
{
    /// <summary>
    /// Adds a menu on Scriptable Objects to swap the script class, if other subclasses exist.
    /// </summary>
    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    public class SwapScriptableObjectTypeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var property = serializedObject.FindProperty("m_Script");
            var scriptAsset = property.objectReferenceValue;
            if (scriptAsset)
            {
                var scriptType = scriptAsset.GetType();
                var assemblyName = (string)scriptType.GetMethod("GetAssemblyName", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(scriptAsset, null);
                assemblyName = assemblyName.Remove(assemblyName.Length - 4);
                var typeName = scriptAsset.name;
                var assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == assemblyName);
                var type = assembly.GetTypes().FirstOrDefault(t => t.Name == typeName);
                var baseType = GetBaseType(type);
                var types = assembly.ExportedTypes.Where(t => t is { IsAbstract: false, ContainsGenericParameters: false } && (t == baseType || IsAssignableFrom(t, baseType))).ToArray();
                if (types.Length > 1)
                {
                    var options = types.Select(t => t.Name).ToArray();
                    var selected = Array.IndexOf(types, type);
                    var newSelected = EditorGUILayout.Popup(selected, options);
                    if (selected == -1 && newSelected == -1) newSelected = 0;

                    if (newSelected != selected)
                    {
                        serializedObject.UpdateIfRequiredOrScript();
                        var selectedType = types[newSelected];
                        var assetGUIDs = AssetDatabase.FindAssets($"{selectedType.Name} t:MonoScript");
                        var asset = assetGUIDs.Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Object>).First(obj => obj.name == selectedType.Name);
                        property.objectReferenceValue = asset;
                        serializedObject.ApplyModifiedProperties();
                        return;
                    }
                }
            }

            base.OnInspectorGUI();
        }

        private static Type GetBaseType(Type type)
        {
            var baseType = type.BaseType;
            if (baseType.IsGenericType) baseType = baseType.GetGenericTypeDefinition();
            return baseType == typeof(ScriptableObject) ? type : GetBaseType(baseType);
        }

        public static bool IsAssignableFrom(Type extendType, Type baseType)
        {
            while (!baseType.IsAssignableFrom(extendType))
            {
                if (extendType == typeof(object)) return false;
                if (extendType.IsGenericType && !extendType.IsGenericTypeDefinition)
                {
                    extendType = extendType.GetGenericTypeDefinition();
                }
                else
                {
                    extendType = extendType.BaseType;
                }
            }
            return true;
        }
    }
}