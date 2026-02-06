using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RaycastTarget : Graphic
{
    public override bool raycastTarget
    {
        get => true;
        set
        {

        }
    }

    public override void SetMaterialDirty()
    {
        return;
    }

    public override void SetVerticesDirty()
    {
        return;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        return;
    }
}

#if UNITY_EDITOR
[CanEditMultipleObjects, CustomEditor(typeof(RaycastTarget), false)]
public class RaycastTargetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.serializedObject.Update();

        //EditorGUILayout.PropertyField (base.m_Script);

        base.serializedObject.ApplyModifiedProperties();
    }
}
#endif
