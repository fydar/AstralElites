using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Splashscreen : MonoBehaviour
{
    public enum State
    {
        Loading,
        Waiting,
        Loaded
    }

    [Header("Animation")]
    public Animator ScreenAnimation;
    public AudioListener audioListener;

    [Header("Parameters")]
    public string SceneName = "Game";

    [Space]
    public State state;

    private IInterpolator ProgressInterpolator;
    private int awaitAnimationID;

    private void Awake()
    {
        ProgressInterpolator = new LinearInterpolator() { Speed = 1.25f };

        awaitAnimationID = Animator.StringToHash("Continue");
    }

    private void Start()
    {
        _ = StartCoroutine(ScreenFlow());
    }

    private void Update()
    {
        ProgressInterpolator.Update(Time.deltaTime);
    }

    private IEnumerator ScreenFlow()
    {
        string sceneToLoad = SceneName;

        state = State.Loading;
        var async = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        async.allowSceneActivation = false;

        while (async.progress != 0.9f)
        {
            ProgressInterpolator.TargetValue = async.progress;
            yield return null;
        }

        state = State.Waiting;

        while (true)
        {
            var animStateInfo = ScreenAnimation.GetCurrentAnimatorStateInfo(0);

            if (animStateInfo.shortNameHash == awaitAnimationID)
            {
                break;
            }

            yield return null;
        }

        audioListener.enabled = false;
        Destroy(audioListener);

        async.allowSceneActivation = true;
        state = State.Loaded;

        _ = SceneManager.UnloadSceneAsync(gameObject.scene);

        gameObject.SetActive(false);
    }
}
