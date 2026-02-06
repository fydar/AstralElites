using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ClockManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text timeText;

    [Header("Clock Hands")]
    [SerializeField] private RectTransform hourHand;
    [SerializeField] private RectTransform minuteHand;

    private void Start()
    {
        StartCoroutine(UpdateClockRoutine());
    }

    private IEnumerator UpdateClockRoutine()
    {
        while (true)
        {
            UpdateClock();

            float secondsUntilNextUpdate = 60 - DateTime.Now.Second;
            yield return new WaitForSeconds(Mathf.Max(1.0f, secondsUntilNextUpdate));
        }
    }

    private void UpdateClock()
    {
        var now = DateTime.Now;

        if (timeText != null)
        {
            timeText.text = now.ToString("HH:mm");
        }

        var timeOfDay = now.TimeOfDay;

        if (hourHand != null)
        {
            float hourTurns = (float)(timeOfDay.TotalHours % 12.0) / 12.0f;
            hourHand.localRotation = Quaternion.Euler(0, 0, hourTurns * -360.0f);
        }

        if (minuteHand != null)
        {
            float minuteTurns = timeOfDay.Minutes / 60f;
            minuteHand.localRotation = Quaternion.Euler(0, 0, minuteTurns * -360.0f);
        }
    }
}
