using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Cursor Style")]
public class CursorStyle : ScriptableObject
{
    public Vector2 Hotspot = Vector2.zero;

    public Texture2D Graphic;
}

#if UNITY_EDITOR
[CustomEditor(typeof(CursorStyle))]
public class CursorStyleEditor : Editor
{
    public override bool RequiresConstantRepaint()
    {
        return true;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var rect = GUILayoutUtility.GetRect(0, 64);
        GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);


        var style = (CursorStyle)target;

        if (rect.Contains(Event.current.mousePosition))
        {
            CursorManager.SetCursor(style);
        }
        else
        {
            if (CursorManager.CurrentStyle != CursorManager.GetStyle("Default"))
            {
                CursorManager.SetCursor(CursorManager.GetStyle("Default"));
            }
        }

        EditorGUIUtility.AddCursorRect(rect, MouseCursor.CustomCursor);
    }
}

#endif
