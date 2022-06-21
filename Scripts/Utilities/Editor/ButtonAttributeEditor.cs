using System.Reflection;
using Unidice.SDK.Utilities;
using UnityEditor;
using UnityEngine;

namespace Unidice.Simulator.Editor.Utilities
{
    /// <summary>
    /// Replaces a field with a button.
    /// </summary>
    [CustomPropertyDrawer(typeof(ButtonAttribute), true)]
    public class ButtonAttributeEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var buttonAttribute = (ButtonAttribute)attribute;
            var methodName = buttonAttribute.MethodName;
            var target = property.serializedObject.targetObject;
            var type = target.GetType();
            var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            if (method == null)
            {
                GUI.Label(position, $"Method '{methodName}' not found on {type.Name}.");
                return;
            }

            if (GUI.Button(position, ObjectNames.NicifyVariableName(method.Name)))
            {
                if (method.GetParameters().Length > 0)
                {
                    method.Invoke(target, buttonAttribute.Parameter);
                }
                else
                {
                    method.Invoke(target, null);
                }
            }
        }
    }
}