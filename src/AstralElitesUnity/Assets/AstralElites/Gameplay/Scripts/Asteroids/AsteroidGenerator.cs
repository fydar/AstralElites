using UnityEngine;

public class AsteroidGenerator : MonoBehaviour
{
    public static AsteroidGenerator instance;

    [Header("Gameplay")]
    public float SpawnCooldown = 0.25f;

    [Header("Asteroids")]
    public AsteroidTemplate[] AsteroidTypes;
    public GameObjectPool<Asteroid> AsteroidPool;

    private void Awake()
    {
        instance = this;

        AsteroidPool.Initialise(null);
    }

    private void Regenerate()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        var template = AsteroidTypes[Random.Range(0, AsteroidTypes.Length)];

        template.Generate(AsteroidPool.Grab(null));
    }

    public void Enable()
    {
        InvokeRepeating(nameof(Regenerate), 1.75f, SpawnCooldown);
    }

    public void Disable()
    {
        if (this == null)
        {
            return;
        }
        CancelInvoke();
    }
}
