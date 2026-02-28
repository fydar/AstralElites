using System.Collections.Generic;
using UnityEngine;

public class PulseEffect : MonoBehaviour, ISerializationCallbackReceiver
{
    public static Dictionary<string, PulseEffect> Instances;

    public string Identifier;

    [Header("Elements")]
    public BunnyReference<SfxGroup>[] AudioAssets;
    public ParticleSystem[] Particles;
    public bool FlipAfterPlay;

    [Header("Camera Shake")]
    public float duration = 0.5f;
    public float speed = 1.0f;
    public float magnitude = 0.1f;
    public AnimationCurve falloff;

    public Vector2 ShakeOffset;

    private float elapsed;
    private float intencity;

    public void OnAfterDeserialize()
    {
        Instances ??= new Dictionary<string, PulseEffect>();
        Instances[Identifier] = this;

        intencity = 0.0f;
        elapsed = 1000000;
    }

    public void OnBeforeSerialize()
    {

    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        float percentComplete = elapsed / duration;
        percentComplete = falloff.Evaluate(percentComplete);

        float damper = Mathf.Max(0.075f, 1.0f - Mathf.Clamp((2.0f * percentComplete) - 1.0f, 0.075f, 0.975f));

        float alpha = speed * percentComplete * intencity;

        float x = (Mathf.PerlinNoise(alpha, 0) * 2.0f) - 1.0f;
        float y = (Mathf.PerlinNoise(0, alpha) * 2.0f) - 1.0f;

        x *= magnitude * damper * intencity;
        y *= magnitude * damper * intencity;

        ShakeOffset = new Vector2(x, y);
    }

    public void PlayAt(Vector3 positon, Quaternion rotation)
    {
        transform.rotation = rotation;

        PlayAt(positon);
    }

    public void PlayAt(Vector3 positon)
    {
        transform.position = positon;

        foreach (var source in AudioAssets)
        {
            AudioManager.Play(source);
        }

        foreach (var particle in Particles)
        {
            particle.Play(true);
        }

        intencity = 1.0f;
        elapsed = 0;

        if (FlipAfterPlay)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }
}
