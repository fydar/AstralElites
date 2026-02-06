using System.Collections;
using UnityEngine;
using Utility;

public class Popup : MonoBehaviour
{
    public CanvasGroup Fader;
    public float FadeInTime = 0.5f;
    public float WaitTime = 1.0f;
    public float FadeOutTime = 0.5f;

    protected IEnumerator PlayRoutine()
    {
        var loop = new TimedLoop(FadeInTime, true);

        foreach (float time in loop)
        {
            Fader.alpha = time;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(WaitTime);

        loop.Duration = FadeOutTime;
        loop.Reset();

        foreach (float time in loop)
        {
            Fader.alpha = 1.0f - time;
            yield return null;
        }

        GetComponentInParent<PopupManager>().GetPopupPool(GetType()).Return(this);
    }
}
