#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Timeframe))]
public class TimeframeDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var MinProperty = property.FindPropertyRelative("Min");
        var MaxProperty = property.FindPropertyRelative("Max");

        float min = MinProperty.floatValue;
        float max = MaxProperty.floatValue;

        EditorGUI.BeginChangeCheck();
        EditorGUI.MinMaxSlider(position, label, ref min, ref max, 0.0f, 1.0f);
        if (EditorGUI.EndChangeCheck())
        {
            MinProperty.floatValue = min;
            MaxProperty.floatValue = max;
        }
    }
}
#endif
