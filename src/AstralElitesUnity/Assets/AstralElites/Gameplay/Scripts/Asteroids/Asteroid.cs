using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Asteroid : MonoBehaviour
{
    public Action OnDestroy;

    [Header("Setup")]
    public MeshFilter body;
    public MeshFilter outline;
    public PolygonCollider2D outlineCollider;

    [Header("Style")]
    public float Width;

    private AsteroidTemplate template;
    private int health;
    private float scale;
    private int segments;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

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

        outlineCollider.points = insidePolygon.Points;
        body.mesh = outlineCollider.CreateMesh(false, false);

        outlineCollider.points = outsidePolygon.Points;
        outline.mesh = outlineCollider.CreateMesh(false, false);

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

    public void Hit(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Kill();
            AudioManager.Play(template.DestroySound);
        }
        else
        {
            AudioManager.Play(template.ImpactSound);
        }
    }

    public void Kill(bool scatterRemains = true)
    {
        DustParticles.Fire(body);
        if (template.Spawn != null && scatterRemains)
        {
            template.Spawn.Scatter(transform.position, scale, rb.linearVelocity * 0.5f, UnityEngine.Random.Range(template.MinSpawn, template.MaxSpawn + 1));
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

    private void Fling(float velocity)
    {
        var direction = -transform.position.normalized;

        var rotator = Quaternion.AngleAxis(UnityEngine.Random.Range(-40.0f, 40.0f), Vector3.forward);

        direction = rotator * direction;

        rb.AddForce(direction * velocity, ForceMode2D.Impulse);
    }

    public static Mesh Trianglulate(Vector2[] points)
    {
        var verts = new Vector3[points.Length + 1];
        var normals = new Vector3[verts.Length];
        verts[0] = Vector3.zero;
        normals[0] = Vector3.up;

        for (int i = 0; i < points.Length; i++)
        {
            verts[i + 1] = points[i];
            normals[i + 1] = Vector3.up;
        }

        var mesh = new Mesh();
        int[] tris = new int[verts.Length * 3];

        int currentIndex = 0;
        for (int i = 1; i < verts.Length; i++)
        {
            tris[currentIndex] = 0;
            tris[currentIndex + 1] = i - 1;
            tris[currentIndex + 2] = i;

            currentIndex += 3;
        }

        tris[currentIndex] = 0;
        tris[currentIndex + 1] = verts.Length - 1;
        tris[currentIndex + 2] = 1;

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.normals = normals;

        return mesh;
    }
}
