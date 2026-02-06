using UnityEngine;
using UnityEngine.UI;

public class AnalyticsRenderer : MonoBehaviour
{
    public AnalyticsManager manager;

    [Header("Rendering")]
    public Text LastCollisionDetails;

    [Space]

    public Text DurationText;
    public Text ScoreText;
    public Text RankText;
    public RectTransform NextRankRow;
    public Text NextRankValueText;

    [Space]

    public RankRender RankDisplay;
    public Text RankAbbreviationText;

    [Space]

    public Text TotalAsteroidsText;
    public Text PerformanceText;
    public Text DistanceTravelledText;
    public Text AverageSpeedText;

    [Space]

    public Chart_Basic ScoreChartA;
    public Chart_Basic ScoreChartB;

    [Space]

    public Chart_Basic HealthChartA;
    public Chart_Basic HealthChartB;

    private GameManager gameManager;

    public void OnEnable()
    {
        gameManager = GameManager.instance;
        if (gameManager == null)
        {
            return;
        }

        ScoreChartA.SetData(manager.scoreHook.data.ToArray());
        ScoreChartB.SetData(manager.scoreHook.data.ToArray());

        if (HealthChartA != null)
        {
            HealthChartA.SetData(manager.healthHook.data.ToArray());
        }
        if (HealthChartB != null)
        {
            HealthChartB.SetData(manager.healthHook.data.ToArray());
        }

        int seconds = ((int)gameManager.GameDuration) % 60;
        int minutes = (int)(gameManager.GameDuration / 60);

        DurationText.text = $"{minutes}m {seconds}s";
        ScoreText.text = gameManager.Score.Value.ToString("###,##0");

        var rank = Rank.GetRank(gameManager.Score.Value);
        var nextRank = Rank.GetNextRank(rank);

        NextRankRow.gameObject.SetActive(nextRank != null);

        if (nextRank != null)
        {
            NextRankValueText.text = $"{nextRank.RequiredScore - gameManager.Score.Value:###,##0} until {nextRank.DisplayName}";
        }

        RankText.text = rank.DisplayName;

        if (RankDisplay != null)
        {
            RankDisplay.RenderRank(rank);
        }
        if (RankAbbreviationText != null)
        {
            RankAbbreviationText.text = rank.Abbreviation;
        }

        TotalAsteroidsText.text = gameManager.AsteroidsDestroyed.Value.ToString();
        PerformanceText.text = $"{gameManager.AsteroidsDestroyed.Value / gameManager.GameDuration:0.00} Asteroids per Second";
        DistanceTravelledText.text = $"{gameManager.DistanceTravelled.Value:###,##0.00} m";
        AverageSpeedText.text = $"{gameManager.DistanceTravelled.Value / gameManager.GameDuration:###,##0.00} m/s";

        if (LastCollisionDetails != null && manager != null)
        {
            LastCollisionDetails.text = $"Your where crused by an asteroid moving {manager.Collisions?.Last?.relativeVelocity.magnitude ?? 0:###,##0.00} m/s.";
        }
    }
}
