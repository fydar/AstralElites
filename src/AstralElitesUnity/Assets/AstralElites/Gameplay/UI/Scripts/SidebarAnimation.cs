using UnityEngine;

public class SidebarAnimation : MonoBehaviour
{
    public float Speed = 2.0f;
    public float FadeSpeed = 8.0f;
    public float FadedAlpha = 0.8f;

    public AnimationCurve Curve;

    [Space]

    public CanvasGroup Parent;
    public RectTransform LittleShip;
    public RectTransform TrailRect;

    private CurveInterpolator Interpolator;
    private DampenInterpolator FaderInterpolator;

    private void Start()
    {
        Interpolator = new CurveInterpolator(Speed, Curve);
        FaderInterpolator = new DampenInterpolator(FadeSpeed);
    }

    private void Update()
    {
        Interpolator.Update(Time.unscaledDeltaTime);
        FaderInterpolator.Update(Time.unscaledDeltaTime);

        LittleShip.anchorMin = new Vector2(0.5f, Interpolator.Value);
        LittleShip.anchorMax = new Vector2(0.5f, Interpolator.Value);
        LittleShip.anchoredPosition3D = Vector3.zero;

        TrailRect.anchorMin = new Vector2(0.0f, 0.0f);
        TrailRect.anchorMax = new Vector2(1.0f, Interpolator.Value);
        TrailRect.offsetMin = Vector2.zero;
        TrailRect.offsetMax = Vector2.zero;

        Parent.alpha = Mathf.Lerp(FadedAlpha, 1.0f, FaderInterpolator.Value);
    }

    public void SetTarget(bool value)
    {
        Interpolator.TargetValue = value ? 1.0f : 0.0f;
        FaderInterpolator.TargetValue = value ? 1.0f : 0.0f;
    }
}
