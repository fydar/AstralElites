using UnityEngine;
using UnityEngine.UI;

public class CanvasMesh : Graphic
{
    public Mesh mesh;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (mesh == null)
        {
            return;
        }

        var boundsSize = mesh.bounds.extents;

        var multiplier = new Vector3(Mathf.Abs(rectTransform.rect.width) / boundsSize.x * -0.5f,
            Mathf.Abs(rectTransform.rect.height) / boundsSize.y * 0.5f, 0);

        var vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            var vertPos = vertices[i];
            vertPos.Scale(multiplier);
            vh.AddVert(vertPos, color, Vector2.zero);
        }

        int[] triangles = mesh.GetTriangles(0);
        for (int i = 0; i < triangles.Length; i += 3)
        {
            vh.AddTriangle(triangles[i], triangles[i + 1], triangles[i + 2]);
        }
    }
}
