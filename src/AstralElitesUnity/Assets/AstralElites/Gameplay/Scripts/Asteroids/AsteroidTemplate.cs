using UnityEngine;

[CreateAssetMenu]
public class AsteroidTemplate : ScriptableObject
{
    public int Health = 1;

    [Space]
    public float MinScale = 0.5f;
    public float MaxScale = 1.0f;

    [Space]

    public float Variation = 0.2f;

    [Space]

    public int MinSegments = 5;
    public int MaxSegments = 9;

    [Header("Spawning")]
    public float MinSpeed = 5.0f;
    public float MaxSpeed = 10.0f;

    [Header("Death")]
    public int Reward = 50;
    [Space]
    public BunnyReference<SfxGroup> DestroySoundAsset;
    public BunnyReference<SfxGroup> ImpactSoundAsset;

    [Header("Spawn on Death")]
    public AsteroidTemplate Spawn;
    public int MinSpawn;
    public int MaxSpawn;

    public void Generate(Asteroid clone)
    {
        clone.Generate(this);
    }

    public void Scatter(Vector3 point, float range, Vector3 velocity, int count = 1)
    {
        for (int i = count - 1; i >= 0; --i)
        {
            var clone = AsteroidGenerator.instance.AsteroidPool.Grab(null);

            clone.Generate(this);

            var rand = Random.insideUnitCircle;

            clone.transform.position = point + (0.5f * range * (Vector3)rand);

            clone.GetComponent<Rigidbody2D>().linearVelocity = velocity;
        }
    }
}
