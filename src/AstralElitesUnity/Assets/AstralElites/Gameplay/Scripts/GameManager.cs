using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public enum PlayState
    {
        Pregame,
        Playing,
        Paused,
        Ended
    }

    public static GameManager instance;

    [SerializeField] private InputActionReference menuInput;


    public bool paused = false;
    public PlayState playState;

    [Header("Values")]
    public GlobalInt Score;
    public GlobalInt Highscore;
    public GlobalInt AsteroidsDestroyed;
    public GlobalFloat DistanceTravelled;

    [Space]

    public float GameDuration = 0.0f;

    [Header("Scene")]
    public Character player;

    [Space]

    public GameObject HUD;
    public GameObject End;
    public GameObject Community;

    private AnalyticsManager analytic;

    public BunnyReference<SfxGroup> OpenPauseMenuAsset;
    public BunnyReference<SfxGroup> ClosePauseMenuAsset;

    public CanvasGroup Fader;
    public float fadeSpeed = 2.5f;

    private IInterpolator interpolator;

    private bool hasStarted = false;

    private void Awake()
    {
        instance = this;
        analytic = GetComponent<AnalyticsManager>();

        interpolator = new LinearInterpolator() { Speed = fadeSpeed };
    }

    private float lastValue;

    private void Start()
    {
        AudioManager.PlayMusic(new BunnyReference<MusicGroup>("music.data", "Main"));

        player.GetComponent<CharacterPlayerController>().enabled = true;

        StartGame();

        lastValue = player.Health.Value;
        player.Health.OnAfterChanged += () =>
        {
            if (player.Health.Value <= 0.0f)
            {
                if (player.isAlive)
                {
                    player.Kill();
                    AudioManager.Play(player.HitSoundAsset);
                    AudioManager.Play(player.DestroySoundAsset);
                    ScreenEffect.Instance.Pulse(1.0f);
                    Camera.main.GetComponent<PerlinShake>().PlayShake(1.0f);
                    EndGame();
                }
            }
            else if (!player.isAlive)
            {
                player.Revive();
            }
            else
            {
                float delta = lastValue - player.Health.Value;
                lastValue = player.Health.Value;
                if (delta > 7)
                {
                    Camera.main.GetComponent<PerlinShake>().PlayShake(Mathf.InverseLerp(-30, 50, delta));
                }

                if (delta > 0)
                {
                    ScreenEffect.Instance.Pulse(delta / 60.0f);
                }
            }
        };
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasStarted)
        {
            Pause();
        }
    }

    private void Update()
    {
        if (playState == PlayState.Playing)
        {
            if (menuInput.action.WasPressedThisFrame())
            {
                UI_TogglePause();
            }

            GameDuration += Time.deltaTime;
        }

        interpolator.Update(Time.unscaledDeltaTime);
        Fader.alpha = interpolator.Value;
        Fader.gameObject.SetActive(interpolator.Value > 0.05f);
    }

    public void StartGame()
    {
        End.SetActive(false);
        HUD.SetActive(true);
        if (Community != null)
        {
            Community.SetActive(false);
        }

        player.Revive();

        Score.Value = 0;
        AsteroidsDestroyed.Value = 0;
        DistanceTravelled.Value = 0;

        AsteroidGenerator.instance.AsteroidPool.Flush();
        AsteroidGenerator.instance.Enable();

        if (DiscordController.Instance != null)
        {
            DiscordController.Instance.StartNewGame();
        }

        analytic.Clear();
        analytic.StartCapture();

        GameDuration = 0.0f;
        playState = PlayState.Playing;
        hasStarted = true;
    }

    public void EndGame()
    {
        HUD.SetActive(false);
        End.SetActive(true);
        if (Community != null)
        {
            Community.SetActive(true);
        }

        AsteroidGenerator.instance.Disable();

        if (DiscordController.Instance != null)
        {
            DiscordController.Instance.EndGame(Score.Value);
        }
        analytic.EndCapture();

        playState = PlayState.Ended;
    }

    public static void ScorePoints(int score)
    {
        if (instance == null)
        {
            return;
        }

        instance.AsteroidsDestroyed.Value += 1;
        instance.Score.Value += score;

        if (instance.Highscore.Value < instance.Score.Value)
        {
            instance.Highscore.Value = instance.Score.Value;
        }
    }

    public void UI_TogglePause()
    {
        if (paused)
        {
            UI_Unpause();
        }
        else
        {
            UI_Pause();
        }
    }

    public void UI_Pause()
    {
		if (playState != PlayState.Paused
			&& playState != PlayState.Ended)
        {
            AudioManager.Play(OpenPauseMenuAsset);
        }
        Pause();
    }

    public void UI_Unpause()
    {
        if (!paused)
        {
            return;
        }

        Time.timeScale = 1.0f;
        paused = false;
        if (Community != null)
        {
            Community.SetActive(false);
        }

        interpolator.TargetValue = 0.0f;

        AudioManager.Play(ClosePauseMenuAsset);
    }

    public void UI_Exit()
    {
        Application.Quit();
    }

    private void Pause()
    {
        if (playState != PlayState.Playing)
        {
            return;
        }

        if (paused)
        {
            return;
        }

        Time.timeScale = 0.0f;
        paused = true;

        if (Community != null)
        {
            Community.SetActive(true);
        }

        interpolator.TargetValue = 1.0f;
    }
}
