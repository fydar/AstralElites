using UnityEngine;
using UnityEngine.UI;

public class Chart_Area : Chart_Basic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        var rect = rectTransform.rect;

        int vertIndex = 0;

        for (int i = 0; i < Points.Length; i++)
        {
            float time = i / ((float)Points.Length - 1);

            float height = Points[i].Value;
            float yPos = Mathf.Lerp(rect.yMin, rect.yMax, height);
            float xPos = Mathf.Lerp(rect.xMin, rect.xMax, time);

            vh.AddVert(new Vector3(xPos, yPos, 0), color, Vector2.zero);
            vh.AddVert(new Vector3(xPos, rect.yMin, 0), color, Vector2.zero);

            if (vertIndex > 1)
            {
                vh.AddTriangle(vertIndex - 2, vertIndex, vertIndex + 1);
                vh.AddTriangle(vertIndex - 2, vertIndex + 1, vertIndex - 1);
            }

            vertIndex += 2;
        }
    }
}
