using UnityEngine;

public class RankPopupManager : MonoBehaviour
{
    public GlobalInt Highscore;

    private Rank lastRank = null;

    private void OnEnable()
    {
        Highscore.OnChanged += OnHighscoreChanged;

        lastRank = Rank.GetRank(Highscore.Value);
    }

    private void OnDisable()
    {
        Highscore.OnChanged -= OnHighscoreChanged;
    }

    private void OnHighscoreChanged()
    {
        var rank = Rank.GetRank(Highscore.Value);

        if (lastRank != rank)
        {
            PopupManager.instance.GetPopup<RankPopup>().DisplayPopup(rank);

            lastRank = rank;
        }
    }
}
