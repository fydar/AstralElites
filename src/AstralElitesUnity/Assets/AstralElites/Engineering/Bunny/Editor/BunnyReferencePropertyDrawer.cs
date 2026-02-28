#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BunnyReference<>))]
public class BunnyReferencePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty bundleProp = property.FindPropertyRelative("assetBundleName");
        SerializedProperty nameProp = property.FindPropertyRelative("assetName");

        Type targetObjectType = GetTargetType();

        UnityEngine.Object currentObject = null;
        if (!string.IsNullOrEmpty(bundleProp.stringValue) && !string.IsNullOrEmpty(nameProp.stringValue))
        {
            string[] paths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleProp.stringValue, nameProp.stringValue);
            if (paths.Length > 0)
            {
                currentObject = AssetDatabase.LoadAssetAtPath(paths[0], targetObjectType);
            }
        }

        EditorGUI.BeginChangeCheck();
        UnityEngine.Object newObject = EditorGUI.ObjectField(position, label, currentObject, targetObjectType, false);

        if (EditorGUI.EndChangeCheck())
        {
            if (newObject == null)
            {
                bundleProp.stringValue = string.Empty;
                nameProp.stringValue = string.Empty;
            }
            else
            {
                string assetPath = AssetDatabase.GetAssetPath(newObject);
                var importer = AssetImporter.GetAtPath(assetPath);

                if (importer != null)
                {
                    bundleProp.stringValue = importer.assetBundleName;
                    nameProp.stringValue = newObject.name;

                    if (string.IsNullOrEmpty(importer.assetBundleName))
                    {
                        Debug.LogWarning($"The asset '{newObject.name}' is not currently assigned to any Asset Bundle.");
                    }
                }
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

        // Handle arrays and lists
        if (type.IsArray)
        {
            type = type.GetElementType();
        }
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
        {
            type = type.GetGenericArguments()[0];
        }

        // Extract T from BunnyReference<T>
        if (type != null && type.IsGenericType)
        {
            return type.GetGenericArguments()[0];
        }

        return typeof(UnityEngine.Object);
    }
}
#endif
