using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

[Serializable]
public class InspectorLog
{
    [Serializable]
    public struct LogItem
    {
        public string content;
        public Action OnClick;

        public LogItem(string message)
        {
            content = message;
            OnClick = null;
        }

        public void Execute()
        {
            Debug.Log("Executing");
            OnClick?.Invoke();
        }
    }

    public List<LogItem> FullLog = new();

    public void Log(string message)
    {
        FullLog.Add(new LogItem(message));
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(InspectorLog))]
public class InspectorLogDrawer : PropertyDrawer
{
    public float scroll = 1.0f;

    public int selectedEntry = -1;

    private Rect lastPosition;
    private int lastLogSize = -1;

    private Rect rect_header,
        rect_content,
        rect_scrollbar;

    private Rect[] rect_logContents;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var logItems = property.FindPropertyRelative("FullLog");

        int logSize = 12;

        float height = EditorGUIUtility.singleLineHeight;

        height += EditorGUIUtility.singleLineHeight * logSize;

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var logItems = property.FindPropertyRelative("FullLog");

        int logItemsCount = logItems.arraySize;
        int logSize = 12;

        if (position != lastPosition || logSize != lastLogSize)
        {
            RecalculateRects(position, logSize);
        }

        if (logItemsCount > logSize)
        {
            float handleSize = Mathf.Max(1 / Mathf.Sqrt(logItemsCount - logSize), 0.1f);

            if (Event.current.type == EventType.ScrollWheel && rect_content.Contains(Event.current.mousePosition))
            {
                if (scroll != 1.0f && Event.current.delta.y < 0 &&
                    scroll != 0.0f && Event.current.delta.y > 0)
                {
                    scroll += Event.current.delta.y * (0.4f / logItemsCount);
                    Event.current.Use();
                }
            }
            scroll = GUI.VerticalScrollbar(rect_scrollbar, scroll, handleSize, 0.0f, 1.0f + handleSize);
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            GUI.VerticalScrollbar(rect_scrollbar, scroll, 1.0f, 0.0f, 1.0f);
            EditorGUI.EndDisabledGroup();

            scroll = 1.0f;
        }

        if (logItemsCount != 0)
        {
            int offset = Mathf.RoundToInt((logItemsCount - logSize) * scroll);

            if (Event.current.type == EventType.MouseDown)
            {
                if (rect_content.Contains(Event.current.mousePosition))
                {
                    float posInContent = Mathf.InverseLerp(rect_content.yMin, rect_content.yMax, Event.current.mousePosition.y);

                    int clickedIndex = Mathf.FloorToInt(posInContent * logSize);
                    int logIndex = clickedIndex + offset;

                    if (Event.current.clickCount == 2)
                    {
                        ExecuteAction(property, logIndex);
                        selectedEntry = logIndex;
                    }
                    else
                    {
                        selectedEntry = logIndex;
                    }

                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                    Event.current.Use();
                }
            }
            else if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.LabelField(rect_header, label, EditorStyles.boldLabel);

                GUI.Box(rect_content, "");

                for (int i = 0; i < rect_logContents.Length; i++)
                {
                    int index = i + offset;

                    if (index < 0)
                    {
                        continue;
                    }

                    if (index >= logItemsCount)
                    {
                        break;
                    }

                    var item = logItems.GetArrayElementAtIndex(index);
                    var itemContent = item.FindPropertyRelative("content");

                    if (selectedEntry == index)
                    {
                        var originalColour = GUI.color;
                        GUI.color = new Color(0.25f, 0.75f, 1.0f, 1.0f);

                        GUI.Box(rect_logContents[i], "");

                        GUI.color = originalColour;
                    }

                    EditorGUI.LabelField(rect_logContents[i], index.ToString() + ": " + itemContent.stringValue);
                }
            }
        }
        else
        {
            GUI.Box(rect_content, "Empty");
        }
    }

    private void RecalculateRects(Rect frame, int logSize)
    {
        rect_header = new Rect(frame)
        {
            height = EditorGUIUtility.singleLineHeight
        };

        rect_content = new Rect(frame);
        rect_content.y += EditorGUIUtility.singleLineHeight;
        rect_content.height -= EditorGUIUtility.singleLineHeight;

        rect_scrollbar = new Rect(rect_content)
        {
            xMin = rect_content.xMax - EditorGUIUtility.singleLineHeight
        };

        rect_content.xMax -= EditorGUIUtility.singleLineHeight;

        rect_logContents = new Rect[logSize];

        var currentLogContent = new Rect(rect_content);

        currentLogContent.xMin += 3;
        currentLogContent.height = EditorGUIUtility.singleLineHeight;

        for (int i = 0; i < logSize; i++)
        {
            rect_logContents[i] = currentLogContent;
            currentLogContent.y += EditorGUIUtility.singleLineHeight;
        }

        lastPosition = frame;
        lastLogSize = logSize;
    }

    private void ExecuteAction(SerializedProperty property, int logItemIndex)
    {
        var logItems = property.FindPropertyRelative("FullLog");
        var item = logItems.GetArrayElementAtIndex(logItemIndex);

        var logItem = (InspectorLog.LogItem)GetTargetObjectOfProperty(item);

        logItem.Execute();
    }


    /// <summary>
    /// Gets the object the property represents.
    /// </summary>
    /// <param name="prop"></param>
    /// <returns></returns>
    public static object GetTargetObjectOfProperty(SerializedProperty prop)
    {
        string path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        string[] elements = path.Split('.');
        foreach (string element in elements)
        {
            if (element.Contains("["))
            {
                string elementName = element[..element.IndexOf("[")];
                int index = System.Convert.ToInt32(element[element.IndexOf("[")..].Replace("[", "").Replace("]", ""));
                obj = GetValue_Imp(obj, elementName, index);
            }
            else
            {
                obj = GetValue_Imp(obj, element);
            }
        }
        return obj;
    }



    private static object GetValue_Imp(object source, string name)
    {
        if (source == null)
        {
            return null;
        }

        var type = source.GetType();

        while (type != null)
        {
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f != null)
            {
                return f.GetValue(source);
            }

            var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p != null)
            {
                return p.GetValue(source, null);
            }

            type = type.BaseType;
        }
        return null;
    }

    private static object GetValue_Imp(object source, string name, int index)
    {
        if (GetValue_Imp(source, name) is not System.Collections.IEnumerable enumerable)
        {
            return null;
        }

        var enm = enumerable.GetEnumerator();
        //while (index-- >= 0)
        //    enm.MoveNext();
        //return enm.Current;

        for (int i = 0; i <= index; i++)
        {
            if (!enm.MoveNext())
            {
                return null;
            }
        }
        return enm.Current;
    }



}
#endif
