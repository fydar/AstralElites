using UnityEngine;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager instance;

    public Character target;

    public AnalyticsHook_Int healthHook;
    public AnalyticsHook_Int scoreHook;
    public AnalyticsHook_Int asteroidsDestroyedHook;
    public AnalyticsHook_Float distanceTravelledHook;

    private GameManager gameManager;
    private AnalyticsHook<Collision2D> collisions;

    public Collision2D LastCollision => collisions?.Last;

    private void Awake()
    {
        gameManager = GetComponent<GameManager>();

        instance = this;
    }

    private void Start()
    {
        healthHook = new AnalyticsHook_Int(gameManager.player.Health);
        scoreHook = new AnalyticsHook_Int(gameManager.Score);
        asteroidsDestroyedHook = new AnalyticsHook_Int(gameManager.AsteroidsDestroyed);
        distanceTravelledHook = new AnalyticsHook_Float(gameManager.DistanceTravelled);

        collisions = new AnalyticsHook<Collision2D>();
        target.OnCollide += collisions.Callback;
    }

    public void Clear()
    {
        scoreHook.Clear();
        healthHook.Clear();
        asteroidsDestroyedHook.Clear();
        distanceTravelledHook.Clear();
    }

    public void StartCapture()
    {
        InvokeRepeating(nameof(Capture), 0.0f, 2.00f);
    }

    public void EndCapture()
    {
        Recapture();
        CancelInvoke(nameof(Capture));
    }

    private void Capture()
    {
        scoreHook.Capture();
        healthHook.Capture();
        asteroidsDestroyedHook.Capture();
        distanceTravelledHook.Capture();
    }

    public void Recapture()
    {
        scoreHook.Recapture();
        healthHook.Recapture();
        asteroidsDestroyedHook.Recapture();
        distanceTravelledHook.Recapture();
    }
}
