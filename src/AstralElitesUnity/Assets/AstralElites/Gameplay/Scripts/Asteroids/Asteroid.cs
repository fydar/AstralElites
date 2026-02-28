using System;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody2D))]
public class Asteroid : MonoBehaviour
{
    public Action OnDestroy;

    [Header("Setup")]
    public MeshFilter body;
    public MeshFilter outline;
    public Rigidbody2D physicsBody;

    public PolygonCollider2D outlineCollider;

    [Header("Style")]
    public float Width;

    public Mesh emissionLine;
    private AsteroidTemplate template;
    private int health;
    private float scale;
    private int segments;

    private void Start()
    {
        InvokeRepeating(nameof(CleanupCheck), 3.0f, 1.0f);
    }

    public void Generate(AsteroidTemplate template)
    {
        this.template = template;

        health = this.template.Health;
        scale = UnityEngine.Random.Range(this.template.MinScale, this.template.MaxScale);
        segments = UnityEngine.Random.Range(this.template.MinSegments, this.template.MaxSegments - 1);

        var outsidePolygon = Polygon.Random(segments, this.template.Variation, scale);
        var insidePolygon = outsidePolygon.Inset(Width);

        body.mesh = Trianglulate(insidePolygon.Points);
        outline.mesh = Trianglulate(outsidePolygon.Points);

        emissionLine = GenerateLineMesh(outsidePolygon.Points);

        outlineCollider.points = outsidePolygon.Points;

        for (int i = 0; i < 10; i++)
        {
            transform.position = ScreenManager.RandomBorderPoint(-outline.mesh.bounds.size);
            if (!Physics2D.OverlapCircle(transform.position, Mathf.Max(outline.mesh.bounds.extents.x, outline.mesh.bounds.extents.y) * 0.5f))
            {
                break;
            }
        }
        Fling(UnityEngine.Random.Range(this.template.MinSpeed, this.template.MaxSpeed));
    }

    public static Mesh GenerateLineMesh(Vector2[] points)
    {
        var mesh = new Mesh
        {
            indexFormat = IndexFormat.UInt16
        };

        int count = points.Length;
        Vector3[] vertices = new Vector3[count];
        Vector3[] normals = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            var current = points[i];
            vertices[i] = new Vector3(current.x, current.y, 0);

            var prev = points[(i + count - 1) % count];
            var next = points[(i + 1) % count];

            var dirIncoming = (current - prev).normalized;
            var dirOutgoing = (next - current).normalized;

            var n1 = new Vector2(-dirIncoming.y, dirIncoming.x);
            var n2 = new Vector2(-dirOutgoing.y, dirOutgoing.x);

            normals[i] = (Vector3)(n1 + n2).normalized;
        }

        int[] indices = new int[count + 1];
        for (int i = 0; i < count; i++)
        {
            indices[i] = i;
        }
        indices[count] = 0; // Close the loop

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.SetIndices(indices, MeshTopology.LineStrip, 0);
        mesh.RecalculateBounds();

        return mesh;
    }

    public static Mesh Trianglulate(Vector2[] points)
    {
        int vertexCount = points.Length + 1;
        var verts = new Vector3[vertexCount];
        var normals = new Vector3[vertexCount];
        verts[0] = Vector3.zero;
        normals[0] = Vector3.back;

        for (int i = 0; i < points.Length; i++)
        {
            verts[i + 1] = points[i];
            normals[i + 1] = Vector3.back;
        }

        var mesh = new Mesh { indexFormat = IndexFormat.UInt16 };
        int triangleIndexCount = points.Length * 3;
        ushort[] tris = new ushort[triangleIndexCount];

        for (int i = 0; i < points.Length; i++)
        {
            int offset = i * 3;
            tris[offset] = 0;
            tris[offset + 1] = (ushort)(i + 1);
            tris[offset + 2] = (ushort)((i + 1 == points.Length) ? 1 : i + 2);
        }

        mesh.vertices = verts;
        mesh.normals = normals;
        mesh.SetIndices(tris, MeshTopology.Triangles, 0);
        mesh.RecalculateBounds();
        mesh.UploadMeshData(false);
        return mesh;
    }

    private void Fling(float velocity)
    {
        var direction = -transform.position.normalized;

        var rotator = Quaternion.AngleAxis(UnityEngine.Random.Range(-40.0f, 40.0f), Vector3.forward);

        direction = rotator * direction;

        physicsBody.AddForce(direction * velocity, ForceMode2D.Impulse);

        // Add a slight random angular velocity to make the movement more natural
        physicsBody.angularVelocity = velocity * UnityEngine.Random.Range(-0.05f, 0.05f) * 360.0f;
    }

    public void Hit(Vector2 impactOrigin, Vector2 impactNormal, int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Kill(impactOrigin, impactNormal);
            AudioManager.Play(template.DestroySoundAsset);
        }
        else
        {
            AudioManager.Play(template.ImpactSoundAsset);
            AsteroidHitParticles.Fire(impactOrigin, impactNormal, 1, this);
        }
    }

    public void Kill(Vector2 impactOrigin, Vector2 impactNormal, bool scatterRemains = true)
    {
        AsteroidHitParticles.Fire(impactOrigin, impactNormal, 1, this);
        AsteroidDestroyedParticles.Fire(impactOrigin, impactNormal, 1, this);
        if (template.Spawn != null && scatterRemains)
        {
            template.Spawn.Scatter(transform.position, scale, physicsBody.linearVelocity * 0.5f, UnityEngine.Random.Range(template.MinSpawn, template.MaxSpawn + 1));
        }

        GameManager.ScorePoints(template.Reward);
        AsteroidGenerator.instance.AsteroidPool.Return(this);

        OnDestroy?.Invoke();
    }

    private void CleanupCheck()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (ScreenManager.IsOutside(transform.position, (-scale) - 0.5f))
        {
            AsteroidGenerator.instance.AsteroidPool.Return(this);
        }
    }
}
