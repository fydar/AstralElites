using UnityEngine;
using UnityEngine.UI;

public class Accordion : MonoBehaviour
{
    public float Speed = 2.0f;
    public AnimationCurve Curve;

    [Header("Content")]
    public LayoutElement Layout;

    [Header("Fade")]
    public Image Fade;
    public Color startColor;
    public Color endColor;

    [Header("Arrow")]
    public RectTransform Arrow;
    public Vector3 startScale;
    public Vector3 endScale;

    public SfxGroup ExpandSound;
    public SfxGroup ContractSound;

    private float StartingHeight = 30.0f;
    private bool Toggled = false;
    private IInterpolator Interpolator;

    private void Awake()
    {
        Interpolator = new CurveInterpolator(Speed, Curve);
        StartingHeight = Layout.preferredHeight;
    }

    private void Update()
    {
        Interpolator.Update(Time.unscaledDeltaTime);

        Layout.preferredHeight = Mathf.Lerp(0, StartingHeight, Interpolator.Value);

        if (Fade != null)
        {
            Fade.color = Color.Lerp(startColor, endColor, Interpolator.Value);
        }

        if (Arrow != null)
        {
            Arrow.transform.localScale = Vector3.Lerp(startScale, endScale, Interpolator.Value);
        }
    }

    public void UiToggle()
    {
        if (Toggled)
        {
            AudioManager.Play(ContractSound);
            Interpolator.TargetValue = 0.0f;
            Toggled = false;
        }
        else
        {
            AudioManager.Play(ExpandSound);
            Interpolator.TargetValue = 1.0f;
            Toggled = true;
        }
    }
}
