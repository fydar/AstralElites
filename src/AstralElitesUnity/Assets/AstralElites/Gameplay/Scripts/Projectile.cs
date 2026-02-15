using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public string EffectId;
    public float Speed = 1.0f;
    public int Damage = 1;
    public float Lifetime = 1.0f;
    public bool ExplodeOnDeath;

    [Header("Area of Effect")]
    public bool HasAreaOfEffect;
    public float AreaOfEffect;

    [Header("Realtime")]
    public GameObject Owner;
    public float LifetimeRemaining;

    private Transform thisTransform;

    [NonSerialized] public GameObjectPool<Projectile> ownerPool;

    private void Awake()
    {
        thisTransform = transform;
    }

    private void Update()
    {
        if (ScreenManager.IsOutside(thisTransform.position))
        {
            DestroyProjectile();
            return;
        }

        LifetimeRemaining -= Time.deltaTime;
        if (LifetimeRemaining <= 0.0f)
        {
            DestroyProjectile();
            return;
        }

        float moveDistance = Speed * Time.deltaTime;

        RaycastHit2D hit;
        hit = Physics2D.Raycast(new Vector2(thisTransform.position.x, thisTransform.position.y),
            new Vector2(thisTransform.right.x, thisTransform.right.y));

        if (hit.transform != null && hit.distance < moveDistance && hit.transform.gameObject != Owner)
        {
            if (hit.transform.gameObject.TryGetComponent<Asteroid>(out var asteroid))
            {
                PulseEffect.Instances[EffectId].PlayAt(hit.point, thisTransform.rotation);
                asteroid.Hit(hit.point, transform.forward, Damage);

                DestroyProjectile();
            }

            if (HasAreaOfEffect)
            {
                var colliders = Physics2D.OverlapCircleAll(hit.point, AreaOfEffect);

                foreach (var collider in colliders)
                {
                    if (collider.gameObject != asteroid.gameObject)
                    {
                        if (collider.gameObject.TryGetComponent<Asteroid>(out var otherAsteroid))
                        {
                            otherAsteroid.Hit(hit.point, transform.right, Damage);
                        }
                    }
                }
            }
        }

        thisTransform.position += thisTransform.right * moveDistance;
    }

    public void DestroyProjectile()
    {
        if (ExplodeOnDeath)
        {
            PulseEffect.Instances[EffectId].PlayAt(thisTransform.position, thisTransform.rotation);
        }

        ownerPool.Return(this);

        if (HasAreaOfEffect)
        {
            var colliders = Physics2D.OverlapCircleAll(transform.position, AreaOfEffect);

            foreach (var collider in colliders)
            {
                if (collider.gameObject.TryGetComponent<Asteroid>(out var otherAsteroid))
                {
                    otherAsteroid.Hit(transform.position, Vector2.up, Damage);
                }
            }
        }
    }
}
