using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Character : MonoBehaviour
{
    public Action<Collision2D> OnCollide;

    public SfxGroup HitSound;
    public SfxGroup DestroySound;
    public SfxGroup WarpSound;
    public SfxGroup GravelHitSound;

    [Space]
    public LoopGroup EngineSound;
    public LoopGroup AlarmSound;
    public LoopGroup ScrapingSound;

    [Header("Health")]
    public EventField<int> Health;
    public AnimationCurve DamageFromVelocity;

    public GlobalFloat DistanceTravelled;

    [Header("Movement")]
    public float MovementSpeed = 10.0f;
    public float RotationSpeed = 10.0f;

    public float border = 0.1f;
    public TrailRenderer[] engineTrails;
    public ParticleSystem engineParticles;

    [Space]

    public float WarpForce = 10000.0f;

    [HideInInspector]
    public bool isAlive = true;

    private bool canFire = true;

    private Rigidbody2D physicsBody;
    private float lastDrag;

    private EffectFader EngineFade;
    private EffectFader AlarmFade;
    private EffectFader ScrapingFade;

    private readonly List<Asteroid> Contacting = new();

    public bool InputIsThrusting { get; set; }
    public Vector2 InputThrust { get; set; }

    public bool InputIsRotating { get; set; }
    public float InputRotationTarget { get; set; }

    public bool IsAttemptingFire => isAlive && canFire;

    private void Awake()
    {
        physicsBody = GetComponent<Rigidbody2D>();

        EngineFade = new EffectFader(new DampenInterpolator() { Speed = 5 })
        {
            TargetValue = 0.0f
        };

        AlarmFade = new EffectFader(new DampenInterpolator() { Speed = 20 })
        {
            TargetValue = 0.0f
        };

        ScrapingFade = new EffectFader(new DampenInterpolator() { Speed = 20 })
        {
            TargetValue = 0.0f
        };

        lastDrag = physicsBody.linearDamping;
    }

    private void Start()
    {
        if (EngineSound != null)
        {
            AudioManager.Play(EngineSound, EngineFade);
        }
        AudioManager.Play(AlarmSound, AlarmFade);
        AudioManager.Play(ScrapingSound, ScrapingFade);
    }

    private Vector3 lastPosition;

    private void Update()
    {
        AlarmFade.TargetValue = Health.Value is < 45 and > 0 ? 1.0f : 0.0f;

        ScrapingFade.TargetValue = Contacting.Count == 0 || Time.timeScale < 0.05f ? 0.0f : 1.0f;

        if (!isAlive)
        {
            AlarmFade.TargetValue = 0.0f;
            ScrapingFade.TargetValue = 0.0f;
            EngineFade.TargetValue = 0.0f;
            return;
        }

        if (InputIsRotating)
        {
            physicsBody.rotation = Mathf.LerpAngle(physicsBody.rotation, InputRotationTarget, Time.deltaTime * RotationSpeed);
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, physicsBody.rotation);
        }

        if (InputIsThrusting)
        {
            if (!engineParticles.isPlaying)
            {
                engineParticles.Play();
            }
            EngineFade.TargetValue = 1.0f;
        }
        else
        {
            if (engineParticles.isPlaying)
            {
                engineParticles.Stop();
            }
            EngineFade.TargetValue = 0.0f;
        }

        if (canFire && isAlive)
        {
            DistanceTravelled.Value += (lastPosition - transform.position).magnitude;
        }

        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (!isAlive)
        {
            return;
        }

        if (canFire)
        {
            ScreenManager.Clamp(transform, border);
        }

        if (InputIsThrusting)
        {
            physicsBody.AddForce(MovementSpeed * Time.fixedDeltaTime * InputThrust, ForceMode2D.Force);
        }

        if (Contacting.Count != 0)
        {
            Health.Value -= 1;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnCollide?.Invoke(collision);

        int damageToTake = Mathf.RoundToInt(DamageFromVelocity.Evaluate(collision.relativeVelocity.magnitude));
        Health.Value = Mathf.Max(0, Health.Value - damageToTake);

        if (collision.gameObject.TryGetComponent<Asteroid>(out var collidingAsteroid))
        {
            if (Health.Value > 0)
            {
                AudioManager.Play(HitSound);
            }

            Contacting.Add(collidingAsteroid);

            collidingAsteroid.OnDestroy += () =>
            {
                _ = Contacting.Remove(collidingAsteroid);
                collidingAsteroid.OnDestroy = null;
            };
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        _ = Contacting.Remove(collision.gameObject.GetComponent<Asteroid>());
    }

    public void Kill()
    {
        engineParticles.Stop();
        isAlive = false;

        lastDrag = physicsBody.linearDamping;
        physicsBody.linearDamping = 0;
        Contacting.Clear();

        foreach (var trail in engineTrails)
        {
            trail.emitting = false;
        }
    }

    public void Revive()
    {
        physicsBody.linearVelocity = Vector2.zero;
        isAlive = true;
        Health.Value = 100;

        physicsBody.linearDamping = lastDrag;
        physicsBody.position = ScreenManager.RandomBorderPoint(-30);
        transform.position = physicsBody.position;
        canFire = false;
        Invoke(nameof(WarpIntoScene), 0.25f);
        Invoke(nameof(StartShooting), 1.25f);
        Contacting.Clear();

        foreach (var trail in engineTrails)
        {
            trail.Clear();
            trail.emitting = true;
        }
    }

    private void WarpIntoScene()
    {
        AudioManager.Play(WarpSound);

        Vector3 direction = -physicsBody.position;

        physicsBody.AddForce(direction * WarpForce, ForceMode2D.Impulse);
    }

    private void StartShooting()
    {
        canFire = true;
        lastPosition = transform.position;
    }
}
