using UnityEngine;

public class FirePoint : MonoBehaviour
{
    [Header("Combat")]
    public float FireCooldown = 0.1f;
    public GameObjectPool<Projectile> WeaponProjectile;
    public SfxGroup ShootSound;

    [SerializeField] private Character _owner;
    private float _nextFireTime;

    private void Awake()
    {
        WeaponProjectile.Initialise(null);
    }

    private void Update()
    {
        if (_owner.IsAttemptingFire && Time.time >= _nextFireTime)
        {
            Fire();
            _nextFireTime = Time.time + FireCooldown;
        }
    }

    private void Fire()
    {
        var projectile = WeaponProjectile.Grab();
        projectile.transform.SetPositionAndRotation(transform.position, transform.rotation);

        if (ShootSound != null)
        {
            AudioManager.Play(ShootSound);
        }

        projectile.LifetimeRemaining = projectile.Lifetime;
        projectile.Owner = gameObject;
        projectile.ownerPool = WeaponProjectile;
    }
}
