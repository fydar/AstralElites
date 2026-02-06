using UnityEngine;
using UnityEngine.UI;

public class Chart_Line : Chart_Basic
{
    public float Thickness = 2.0f;

    [Range(0, 1)]
    public float Balance = 0.0f;

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

            vh.AddVert(new Vector3(xPos, yPos + (Thickness * Balance), 0), color, Vector2.zero);
            vh.AddVert(new Vector3(xPos, yPos - (Thickness * (1.0f - Balance)), 0), color, Vector2.zero);

            if (vertIndex > 1)
            {
                vh.AddTriangle(vertIndex - 2, vertIndex, vertIndex + 1);
                vh.AddTriangle(vertIndex - 2, vertIndex + 1, vertIndex - 1);
            }

            vertIndex += 2;
        }
    }
}
