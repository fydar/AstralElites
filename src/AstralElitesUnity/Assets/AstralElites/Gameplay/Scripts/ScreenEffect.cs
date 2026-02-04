using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class ScreenEffect : MonoBehaviour
{
    public static ScreenEffect Instance { get; private set; }

    [Header("Settings")]
    public Spring Spring;
    public float PulseVelocity = 10f;

    [Tooltip("The minimum weight value before the volume is disabled.")]
    public float SleepThreshold = 0.001f;

    private Volume _volume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _volume = GetComponent<Volume>();

        UpdateVolumeState(Mathf.Abs(Spring.Value));
    }

    private void Update()
    {
        Spring.Update(Time.deltaTime);

        float currentWeight = Mathf.Abs(Spring.Value);
        UpdateVolumeState(currentWeight);
    }

    public void Pulse(float strength)
    {
        Spring.Velocity += strength * PulseVelocity;
    }

    private void UpdateVolumeState(float weight)
    {
        // If the spring is effectively at rest and the weight is near zero
        if (weight < SleepThreshold && Mathf.Abs(Spring.Velocity) < SleepThreshold)
        {
            if (_volume.enabled)
            {
                _volume.weight = 0;
                _volume.enabled = false;
            }
        }
        else
        {
            if (!_volume.enabled)
            {
                _volume.enabled = true;
            }

            _volume.weight = weight;
        }
    }
}
