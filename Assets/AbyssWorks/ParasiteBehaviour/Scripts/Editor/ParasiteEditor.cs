using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AbyssWorks.ParasiteBehaviour
{
    [CustomPropertyDrawer(typeof(ParasiteBehaviour), true)]
    public class ParasiteEditor : PropertyDrawer
    {
        private Type[] types = null;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Lazy-load Ability subclasses
            if (types == null)
            {
                types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => typeof(ParasiteBehaviour).IsAssignableFrom(t) && !t.IsAbstract)
                    .ToArray();
            }

            if (types == null || types.Length == 0) return;

            // Get current type
            Type currentType = property.managedReferenceValue?.GetType();
            
            /*if (currentType == null)
            {
                Type defaultType = types.FirstOrDefault(t => t == typeof(ParasiteBehaviour)) ?? types[0];

                property.managedReferenceValue = Activator.CreateInstance(defaultType);

                currentType = defaultType;
            }*/

            int currentIndex = Array.IndexOf(types, currentType);

            // Draw the dropdown
            int newIndex = EditorGUI.Popup(
                new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                label.text,
                currentIndex,
                types.Select(t => t.Name).ToArray()
            );

            if (newIndex != currentIndex)
            {
                property.managedReferenceValue = Activator.CreateInstance(types[newIndex]);
            }

            // Move rect down for the actual serialized fields
            Rect fieldRect = new Rect(
                position.x,
                position.y + EditorGUIUtility.singleLineHeight + 2, // 2px spacing
                position.width,
                position.height - EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(fieldRect, property, GUIContent.none, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Base height of dropdown
            float height = EditorGUIUtility.singleLineHeight + 2;

            if (property.managedReferenceValue != null)
            {
                // Add the height of the serialized fields of the selected parasite object
                height += EditorGUI.GetPropertyHeight(property, GUIContent.none, true);
            }
            height += 2;
            return height;
        }
    }
}

