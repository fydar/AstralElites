using System;
using UnityEngine;

public readonly struct Polygon
{
    public readonly Vector2[] Points;

    public Polygon(Vector2[] points)
    {
        Points = points;
    }

    public Polygon Inset(float amount)
    {
        int count = Points.Length;
        var newPoints = new Vector2[count];

        for (int i = 0; i < count; i++)
        {
            var pPrev = Points[(i + count - 1) % count];
            var pCurr = Points[i];
            var pNext = Points[(i + 1) % count];

            var dirLeft = (pCurr - pPrev).normalized;
            var dirRight = (pNext - pCurr).normalized;

            var nLeft = new Vector2(-dirLeft.y, dirLeft.x);
            var nRight = new Vector2(-dirRight.y, dirRight.x);

            var miter = (nLeft + nRight).normalized;

            float dot = Vector2.Dot(miter, nLeft);

            if (Math.Abs(dot) < 0.001f)
            {
                newPoints[i] = pCurr - (nLeft * amount);
            }
            else
            {
                float miterLength = amount / dot;
                if (miterLength > amount * 10f)
                {
                    miterLength = amount * 10f;
                }
                newPoints[i] = pCurr - (miter * miterLength);
            }
        }

        return new Polygon(newPoints);
    }

    public static Polygon Random(int segments, float variation, float scale)
    {
        float spacing = 360.0f / segments;

        var rotator = Quaternion.AngleAxis(spacing, Vector3.up);

        var points = new Vector2[segments];

        var current = Vector3.forward;
        for (int i = 0; i < segments; i++)
        {
            float distance = Mathf.Lerp(variation, 1.0f, UnityEngine.Random.value) * scale;
            var rot = current * distance;
            points[i] = new Vector2(rot.x, rot.z);

            current = rotator * current;
        }

        return new Polygon(points);
    }
}
