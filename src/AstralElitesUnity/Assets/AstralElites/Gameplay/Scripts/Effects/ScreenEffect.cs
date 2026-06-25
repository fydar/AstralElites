using UnityEngine;

public class ScreenEffect : MonoBehaviour
{
    public static ScreenEffect Instance { get; private set; }

    [Header("Settings")]
    public Spring Spring;
    public float PulseVelocity = 10f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        Spring.Update(Time.deltaTime);

        if (BloomRenderFeature.Instance != null)
        {
            BloomRenderFeature.Instance.IntensityMultiplier = Mathf.Clamp(Mathf.Abs(Spring.Value), 0, 1);
        }
    }

    public void Pulse(float strength)
    {
        Spring.Velocity += strength * PulseVelocity;
    }
}
