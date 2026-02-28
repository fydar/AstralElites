#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AstralElites.Gameplay
{
    public class TimeWindow : EditorWindow
    {
        [SerializeField]
        private bool compensatePhysics = false;

        [SerializeField]
        private float targetTimeScale = 1.0f;

        [MenuItem("Toolkit/Time")]
        private static void Open()
        {
            var window = GetWindow<TimeWindow>();
            window.Show();

            window.titleContent = new GUIContent("Time");
            window.minSize = new Vector2(196, 64);
        }

        private void Update()
        {
            if (!EditorApplication.isPlaying)
            {
                return;
            }

            // Intercept standard unpause: Override 1 with our selected time.
            // By strictly checking for 1.0f, we safely ignore 0.0f (paused).
            if (Mathf.Approximately(Time.timeScale, 1.0f) && !Mathf.Approximately(targetTimeScale, 1.0f))
            {
                SetTimeScale(targetTimeScale);
            }
        }

        private void OnGUI()
        {
            EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("0.25x", EditorStyles.toolbarButton))
            {
                SetTargetTimeScale(0.25f);
            }
            if (GUILayout.Button("0.5x", EditorStyles.toolbarButton))
            {
                SetTargetTimeScale(0.5f);
            }

            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.1f, 0.925f, 0.55f);
            if (GUILayout.Button("1x", EditorStyles.toolbarButton))
            {
                SetTargetTimeScale(1.0f);
            }
            GUI.backgroundColor = originalColor;

            if (GUILayout.Button("1.5x", EditorStyles.toolbarButton))
            {
                SetTargetTimeScale(1.5f);
            }
            if (GUILayout.Button("2x", EditorStyles.toolbarButton))
            {
                SetTargetTimeScale(2.0f);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);
            EditorGUI.BeginChangeCheck();
            float newTargetTimeScale = EditorGUILayout.Slider("Target Time Scale", targetTimeScale, 0.1f, 2.0f);
            if (EditorGUI.EndChangeCheck())
            {
                SetTargetTimeScale(newTargetTimeScale);
            }
            EditorGUILayout.Space(6);

            EditorGUI.BeginChangeCheck();
            compensatePhysics = EditorGUILayout.Toggle("Compensate Physics", compensatePhysics);
            if (EditorGUI.EndChangeCheck())
            {
                if (Time.timeScale != 0f)
                {
                    SetTimeScale(targetTimeScale);
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        private void SetTargetTimeScale(float newScale)
        {
            targetTimeScale = newScale;

            // Only apply immediately if the game isn't currently paused
            if (Time.timeScale != 0f)
            {
                SetTimeScale(targetTimeScale);
            }
        }

        private void SetTimeScale(float timeScale)
        {
            Time.timeScale = timeScale;

            if (compensatePhysics)
            {
                Time.fixedDeltaTime = 0.02f * Mathf.Clamp(timeScale, 0.0f, 8.0f);
            }
            else
            {
                var simpleAcceleration = Mathf.Clamp(timeScale, 1.0f, 2.0f);
                Time.fixedDeltaTime = 0.02f * timeScale / simpleAcceleration;
            }
        }
    }
}
#endif
