using UnityEngine;
using UnityEngine.UI;

public class RankPopup : Popup
{
    [Header("Rank")]
    public Text Name;
    public RankRender RankDisplay;
    public Text Description;

    public void DisplayPopup(Rank rank)
    {
        Name.text = rank.DisplayName;
        RankDisplay.RenderRank(rank);
        Description.text = $"Score {rank.RequiredScore:###,##0}";

        _ = StartCoroutine(PlayRoutine());
    }
}
