using System;
using System.Collections.Generic;
using System.Linq;
using Unidice.SDK.Utilities;
using UnityEditor;
using UnityEngine;

namespace Unidice.Simulator.Editor.Utilities
{
    /// <summary>
    /// Adds a menu on "Swappable" fields to swap the script class, if other subclasses exist.
    /// </summary>
    [CustomPropertyDrawer(typeof(SwappableAttribute), true)]
    public class SwappableSerializeReferenceTypeEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var rectLower = new Rect(position);
            rectLower.yMin += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
            var rectUpper = new Rect(position);
            rectUpper.height = EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;

            var type = fieldInfo.FieldType;

            if (type.HasElementType)
            {
                type = type.GetElementType();
                //property.NextVisible(true);
            }
            else
            {
                var genericArguments = type.GetGenericArguments();
                if (genericArguments.Length > 0)
                {
                    type = genericArguments[0];
                    //property.NextVisible(true);
                }
            }
            var assembly = type.Assembly;

            var types = assembly.DefinedTypes.Where(t => t is { IsAbstract: false } && IsMatchingType(t, type)).ToArray();
            if (types.Length > 1)
            {
                var options = types.Select(t => t.Name).ToArray();
                int selected;
                
                if (property.propertyType == SerializedPropertyType.ManagedReference)
                {
                    var currentTypeName = property.managedReferenceFullTypename.Split(' ').Last();
                    selected = Array.IndexOf(types.Select(t => t.FullName).ToArray(), currentTypeName);
                }
                else
                {
                    Debug.LogError($"Swappable fields have to also use the SerializedReference attribute ({property.propertyPath}) and are not supported for object references.");
                    return;
                }

                var rectPopup = new Rect(rectUpper);
                rectPopup.xMin += 8;
                var newSelected = EditorGUI.Popup(rectPopup, selected, options);
                if (selected == -1 && newSelected == -1) newSelected = 0;

                if (newSelected != selected)
                {
                    var selectedType = types[newSelected];
                    ApplyNewType(selectedType, property);
                    property.isExpanded = true;
                    return;
                }
            }

            if (EditorGUI.PropertyField(rectUpper, property, false))
            {
                EditorGUI.indentLevel++;
                bool first = true;
                
                var startingDepth = property.depth;
                while (property.NextVisible(first) && !SerializedProperty.EqualContents(property, property.GetEndProperty()) && property.depth > startingDepth)
                {
                    EditorGUI.PropertyField(rectLower, property, true);
                    rectLower.y += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(property);
                    first = false;
                }
                EditorGUI.indentLevel--;
            }
        }

        private static bool IsMatchingType(Type instanceType, Type baseType)
        {
            if (instanceType == baseType) return true;
            if (baseType.IsAssignableFrom(instanceType)) return true;
            return false;
        }

        private static Type GetBaseType(Type type)
        {
            var baseType = type.BaseType;
            return baseType == typeof(object) ? type : GetBaseType(baseType);
        }

        private static void ApplyNewType(Type type, SerializedProperty property)
        {
            if (type == null) return;
            property.serializedObject.Update();
            object instance;
            if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
                instance = ScriptableObject.CreateInstance(type);
            }
            else
            {
                instance = Activator.CreateInstance(type);
            }

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                throw new NotSupportedException($"ObjectReference {property.serializedObject.targetObject.GetType().Name} {property.type}");
            }
            else if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                property.managedReferenceValue = instance;
            }
            else
            {
                // Not supported
                throw new NotSupportedException();
            }

            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static (string AssemblyName, string ClassName) GetAssemblyAndClass(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return (null, null);

            var typeSplitString = typeName.Split(char.Parse(" "));
            var classSplitString = typeSplitString[1].Split('/', '.');
            return (typeSplitString[0], classSplitString[classSplitString.Length - 1]);
        }
    }
}