#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Spring))]
public class SpringDrawer : PropertyDrawer
{
    private readonly int samples = 32;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 180;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var springPowerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        var springDamperRect = new Rect(position.x, springPowerRect.yMax + EditorGUIUtility.standardVerticalSpacing,
            position.width, EditorGUIUtility.singleLineHeight);

        var remainingRect = new Rect(position.x, springDamperRect.yMax + EditorGUIUtility.standardVerticalSpacing,
            position.width, position.yMax - springDamperRect.yMax);


        var springPowerProperty = property.FindPropertyRelative("Power");
        var springDamperProperty = property.FindPropertyRelative("Damper");

        EditorGUI.PropertyField(springPowerRect, springPowerProperty);
        EditorGUI.PropertyField(springDamperRect, springDamperProperty);

        if (Event.current.type == EventType.Repaint)
        {
            //EditorStyles.helpBox.Draw(remainingRect, GUIContent.none, 0);
            EditorGUI.DrawRect(remainingRect, new Color(0.1f, 0.1f, 0.1f, 1.0f));

            var cache = new List<float>();

            var spring = new Spring
            {
                Power = springPowerProperty.floatValue,
                Damper = springDamperProperty.floatValue
            };

            for (int i = 0; i < samples; i++)
            {
                cache.Add(spring.Value);

                spring.Update(1.0f / 60.0f);
            }

            Handles.BeginGUI();
            var graphInfo = new Vector3[cache.Count];

            for (int i = 0; i < cache.Count; i++)
            {
                float time = ((float)i) / (cache.Count - 1);
                float value = cache[i];


                graphInfo[i] = new Vector3(Mathf.Lerp(remainingRect.xMin, remainingRect.xMax, time),
                    Mathf.Lerp(remainingRect.yMax, remainingRect.yMin, value), 0);

                if (i != 0)
                {
                    Handles.DrawLine(graphInfo[i - 1], graphInfo[i]);
                }
            }

            Handles.color = Color.green;
            Handles.DrawPolyLine(graphInfo);

            Handles.EndGUI();

        }
    }
}
#endif
