using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Values/Prefs Bool")]
public class PrefsBool : GlobalBool
{
    public string PlayerPrefsKey = "Key";
    public bool DefaultValue = false;

    private bool HasLoaded = false;

    public override bool Value
    {
        get
        {
            if (!HasLoaded)
            {
                currentValue = GetBool(PlayerPrefsKey, DefaultValue);
                HasLoaded = true;
            }
            return currentValue;
        }
        set
        {
            currentValue = value;
            SetBool(PlayerPrefsKey, currentValue);

            OnChanged?.Invoke();
        }
    }

    public static void SetBool(string key, bool state)
    {
        PlayerPrefs.SetInt(key, state ? 1 : 0);
    }

    public static bool GetBool(string key, bool defaultValue = false)
    {
        int value = PlayerPrefs.GetInt(key, defaultValue ? 1 : 0);
        return value == 1;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PrefsBool))]
    public class PrefsBoolEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var prefsBool = (PrefsBool)target;

            EditorGUILayout.LabelField("Current Value", PrefsBool.GetBool(prefsBool.PlayerPrefsKey, false).ToString());

            if (GUILayout.Button("Clear"))
            {
                PlayerPrefs.DeleteKey(prefsBool.PlayerPrefsKey);
                prefsBool.HasLoaded = false;
                prefsBool.currentValue = false;
            }
        }
    }
#endif
}
