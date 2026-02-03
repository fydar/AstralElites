using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class IntAnimator : MonoBehaviour
{
    [Header("Settings")]
    public GlobalInt valueSource;
    public float interpolationSpeed = 10f;

    private Text textComponent;
    private float currentFloatValue;
    private int lastDisplayedValue;

    private void Awake()
    {
        textComponent = GetComponent<Text>();
    }

    private void OnEnable()
    {
        currentFloatValue = valueSource.Value;
        lastDisplayedValue = Mathf.RoundToInt(currentFloatValue);
        textComponent.text = lastDisplayedValue.ToString();
    }

    private void Update()
    {
        if (Mathf.Abs(currentFloatValue - valueSource.Value) > 0.01f)
        {
            currentFloatValue = Mathf.Lerp(
                currentFloatValue,
                valueSource.Value,
                Time.deltaTime * interpolationSpeed
            );

            int roundedValue = Mathf.RoundToInt(currentFloatValue);
            if (roundedValue != lastDisplayedValue)
            {
                textComponent.text = roundedValue.ToString();
                lastDisplayedValue = roundedValue;
            }
        }
        else
        {
            currentFloatValue = valueSource.Value;
        }
    }
}