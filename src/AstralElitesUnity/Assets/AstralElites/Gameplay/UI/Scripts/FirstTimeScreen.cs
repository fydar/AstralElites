using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utility;

public class FirstTimeScreen : MonoBehaviour
{
    public PrefsBool DiscordTOS;

    [Space]
    public AudioListener listener;
    public CanvasGroup Fader;
    public Toggle ToggleValue;
    public Button ContinueButton;

    [Header("Transition")]
    public string SceneName;

    [Space]
    public float FadeInTime = 0.5f;
    public float FadeOutTime = 0.5f;

    private AsyncOperation async;

    public void UI_Close()
    {
        if (ToggleValue.isOn)
        {
            DiscordTOS.Value = true;
            StartCoroutine(CloseRoutine());
        }
    }

    private void Start()
    {
        StartCoroutine(FadeIn());

        Time.timeScale = 1.0f;
    }

    private IEnumerator FadeIn()
    {
        async = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
        async.allowSceneActivation = false;

        var loop = new TimedLoop(FadeInTime);

        foreach (float time in loop)
        {
            Fader.alpha = time;
            yield return null;
        }
    }

    private IEnumerator CloseRoutine()
    {
        var loop = new TimedLoop(FadeInTime);

        foreach (float time in loop)
        {
            Fader.alpha = 1.0f - time;
            yield return null;
        }

        listener.enabled = false;
        Destroy(listener);

        async.allowSceneActivation = true;

        while (!async.isDone)
        {
            yield return null;
        }

        var asyncUnload = SceneManager.UnloadSceneAsync(gameObject.scene);
    }
}
