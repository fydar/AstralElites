using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class LazerParticles : MonoBehaviour
{
    private static LazerParticles instance;

    private ParticleSystem particles;

    private void Awake()
    {
        instance = this;
        particles = GetComponent<ParticleSystem>();
    }

    public void FireOnTarget(Vector3 position, Quaternion rotation)
    {
        particles.transform.SetPositionAndRotation(position, rotation);

        particles.Emit(3);
    }

    public static void Fire(Vector3 position, Quaternion rotation)
    {
        instance.FireOnTarget(position, rotation);
    }
}
