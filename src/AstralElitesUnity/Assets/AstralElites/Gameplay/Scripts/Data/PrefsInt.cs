using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Values/Prefs Int")]
public class PrefsInt : GlobalInt
{
    public string PlayerPrefsKey = "Key";

    private bool HasLoaded = false;

    public override int Value
    {
        get
        {
            if (!HasLoaded)
            {
                currentValue = PlayerPrefs.GetInt(PlayerPrefsKey, 0);
                HasLoaded = true;
            }
            return currentValue;
        }
        set
        {
            currentValue = value;
            PlayerPrefs.SetInt(PlayerPrefsKey, currentValue);

            OnChanged?.Invoke();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PrefsInt))]
    public class PrefsIntEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var prefsBool = (PrefsInt)target;

            EditorGUILayout.LabelField("Current Value", PlayerPrefs.GetInt(prefsBool.PlayerPrefsKey).ToString());

            if (GUILayout.Button("Clear"))
            {
                PlayerPrefs.DeleteKey(prefsBool.PlayerPrefsKey);
                prefsBool.HasLoaded = false;
                prefsBool.currentValue = 0;
            }
        }
    }
#endif
}
