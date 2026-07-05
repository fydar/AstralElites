#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Asteroid))]
public class AsteroidEditor : Editor
{
    private static AsteroidTemplate asteroidTemplate;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        asteroidTemplate = (AsteroidTemplate)EditorGUILayout.ObjectField(new GUIContent("Preview Template"),
            asteroidTemplate, typeof(AsteroidTemplate), false);

        EditorGUI.BeginDisabledGroup(asteroidTemplate == null);
        if (GUILayout.Button("Preview"))
        {
            var asteroid = (Asteroid)target;

            asteroid.Generate(asteroidTemplate);
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndVertical();
    }
}
#endif
