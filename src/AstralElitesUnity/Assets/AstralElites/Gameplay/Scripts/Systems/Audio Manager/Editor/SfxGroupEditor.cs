#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

[CustomEditor(typeof(SfxGroup)), CanEditMultipleObjects]
public class SfxGroupEditor : Editor
{
    public string lastPlayed = "";

    [OnOpenAsset]
    public static bool OpenAsset(int entityId, int line)
    {
        var asset = EditorUtility.EntityIdToObject(entityId);

        if (asset is SfxGroup group)
        {
            PreviewGroup(group);
            return true;
        }
        return false;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        if (GUILayout.Button("Play Clips"))
        {
            foreach (var t in targets)
            {
                if (t is SfxGroup group)
                {
                    lastPlayed = PreviewGroup(group).name;
                }
            }
        }

        if (targets.Length > 1)
        {
            EditorGUILayout.LabelField("Playing multiple clips...");
        }
        else
        {
            EditorGUILayout.LabelField("Last Played:", lastPlayed);
        }
    }

    public static AudioClip PreviewGroup(SfxGroup group)
    {
        var clip = group.GetClip();
        if (clip != null)
        {
            PlayClip(clip, group);
        }
        return clip;
    }

    public static void PlayClip(AudioClip clip, SfxGroup group)
    {
        var go = EditorUtility.CreateGameObjectWithHideFlags("PLAY_AUDIO_TEMP", HideFlags.HideAndDontSave);

        var source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = UnityEngine.Random.Range(group.VolumeRange.x, group.VolumeRange.y);
        source.pitch = UnityEngine.Random.Range(group.PitchRange.x, group.PitchRange.y);
        source.priority = group.Priority;
        source.Play();
    }
}
#endif
