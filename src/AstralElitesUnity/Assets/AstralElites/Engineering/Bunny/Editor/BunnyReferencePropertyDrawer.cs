#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Fydar.Bunny.Editor
{
    [CustomPropertyDrawer(typeof(BunnyReference<>))]
    public class BunnyReferencePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty bundleProp = property.FindPropertyRelative("assetBundleName");
            SerializedProperty nameProp = property.FindPropertyRelative("assetName");

            Type targetType = GetTargetType();

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            Rect bundleRect = new Rect(position.x, position.y, position.width, lineHeight);
            Rect assetRect = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);

            // Bundle Dropdown
            string[] allBundleNames = AssetDatabase.GetAllAssetBundleNames();
            string currentBundle = bundleProp.stringValue;

            int bundleIndex = Array.IndexOf(allBundleNames, currentBundle);
            string[] bundleOptions = new string[allBundleNames.Length + 1];
            bundleOptions[0] = "(None)";
            allBundleNames.CopyTo(bundleOptions, 1);

            Rect bundleControlRect = EditorGUI.PrefixLabel(bundleRect, label);

            EditorGUI.BeginChangeCheck();
            int newBundleIndex = EditorGUI.Popup(bundleControlRect, bundleIndex + 1, bundleOptions);
            if (EditorGUI.EndChangeCheck())
            {
                bundleProp.stringValue = newBundleIndex == 0 ? string.Empty : bundleOptions[newBundleIndex];
                nameProp.stringValue = string.Empty;
            }

            // Asset Dropdown
            string selectedBundle = bundleProp.stringValue;
            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(selectedBundle)))
            {
                var assetNames = new List<string>();
                if (!string.IsNullOrEmpty(selectedBundle))
                {
                    foreach (string path in AssetDatabase.GetAssetPathsFromAssetBundle(selectedBundle))
                    {
                        var asset = AssetDatabase.LoadAssetAtPath(path, targetType);
                        if (asset != null)
                            assetNames.Add(asset.name);
                    }
                }

                string currentAsset = nameProp.stringValue;
                int assetIndex = assetNames.IndexOf(currentAsset);

                string[] assetOptions = new string[assetNames.Count + 1];
                assetOptions[0] = "(None)";
                assetNames.CopyTo(assetOptions, 1);

                Rect assetControlRect = new Rect(
                    assetRect.x + EditorGUIUtility.labelWidth,
                    assetRect.y,
                    assetRect.width - EditorGUIUtility.labelWidth,
                    assetRect.height
                );

                EditorGUI.BeginChangeCheck();
                int newAssetIndex = EditorGUI.Popup(assetControlRect, assetIndex + 1, assetOptions);
                if (EditorGUI.EndChangeCheck())
                {
                    nameProp.stringValue = newAssetIndex == 0 ? string.Empty : assetOptions[newAssetIndex];
                }
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Safely extracts the type 'T' from the generic struct, accounting for Arrays and Lists.
        /// </summary>
        private Type GetTargetType()
        {
            Type type = fieldInfo.FieldType;

            if (type.IsArray)
                type = type.GetElementType();
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
                type = type.GetGenericArguments()[0];

            if (type != null && type.IsGenericType)
                return type.GetGenericArguments()[0];

            return typeof(UnityEngine.Object);
        }
    }
}
#endif
