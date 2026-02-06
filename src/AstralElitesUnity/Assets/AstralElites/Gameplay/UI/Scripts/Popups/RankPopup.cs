using UnityEngine;
using UnityEngine.UI;

public class RankPopup : Popup
{
    [Header("Rank")]
    public Text Name;
    public RankRender RankDisplay;

    public void DisplayPopup(Rank rank)
    {
        Name.text = rank.DisplayName;
        RankDisplay.RenderRank(rank);

        _ = StartCoroutine(PlayRoutine());
    }
}
