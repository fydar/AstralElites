using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class AsteroidDestroyedParticles : MonoBehaviour, ISerializationCallbackReceiver
{
    private static AsteroidDestroyedParticles instance;

    private static ParticleSystem.Particle[] cache;

    public Vector3 Offset;

    [SerializeField]
    private int emissionCount;

    private ParticleSystem particles;

    public void OnAfterDeserialize()
    {
        instance = this;
    }

    public void OnBeforeSerialize()
    {
    }

    private void Start()
    {
        if (particles == null)
        {
            particles = GetComponent<ParticleSystem>();
            cache = new ParticleSystem.Particle[particles.main.maxParticles];
        }

        particles.Stop();
    }

    public void PlayOnAsteroid(Vector2 impactOrigin, Vector2 impactNormal, float impactPower, Asteroid asteroid)
    {
        int emitCount = Mathf.RoundToInt(emissionCount * asteroid.emissionLine.bounds.extents.magnitude);
        var shape = particles.shape;
        shape.mesh = asteroid.outline.mesh;

        particles.transform.SetPositionAndRotation(asteroid.transform.position + Offset, asteroid.transform.rotation);

        particles.Emit(emitCount);
        int totalActive = particles.GetParticles(cache);
        int startIndex = totalActive - emitCount;

        for (int i = 0; i < emitCount; i++)
        {
            int index = startIndex + i;
            if (index < 0 || index >= totalActive) continue;

            Vector3 worldPos;
            if (particles.main.simulationSpace == ParticleSystemSimulationSpace.World)
            {
                worldPos = cache[index].position;
            }
            else
            {
                worldPos = particles.transform.TransformPoint(cache[index].position);
            }

            var totalPointVelocity = asteroid.physicsBody.GetPointVelocity(worldPos);

            cache[index].velocity += (Vector3)totalPointVelocity * 1f;
        }

        particles.SetParticles(cache, totalActive);
    }

    public static void Fire(Vector2 impactOrigin, Vector2 impactNormal, float impactPower, Asteroid asteroid)
    {
        instance.PlayOnAsteroid(impactOrigin, impactNormal, impactPower, asteroid);
    }
}
