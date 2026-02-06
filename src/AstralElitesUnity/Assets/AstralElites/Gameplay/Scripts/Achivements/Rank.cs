using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rank")]
public class Rank : ScriptableObject
{
    public string DisplayName = "Asset";
    public string Abbreviation = "Asset";
    public string DiscordAsset = "Asset";
    public int RequiredScore = 1000;

    public RankRenderInformation Render;

    private static Rank[] Ranks;

    public static Rank GetRank(int score)
    {
        EnsureRanksInitialized();

        if (Ranks.Length == 0) return null;

        var lastRank = Ranks[0];

        for (int i = 1; i < Ranks.Length; i++)
        {
            var currentRank = Ranks[i];

            if (currentRank.RequiredScore > score)
            {
                return lastRank;
            }
            lastRank = currentRank;
        }
        return lastRank;
    }

    public static Rank GetNextRank(Rank previousRank)
    {
        EnsureRanksInitialized();

        bool found = false;
        foreach (var rank in Ranks)
        {
            if (rank == previousRank)
            {
                found = true;
            }
            else if (found)
            {
                return rank;
            }
        }

        return null;
    }

    private static void EnsureRanksInitialized()
    {
        if (Ranks != null) return;

        var ranksList = new List<Rank>(Resources.LoadAll<Rank>("Achievements"));
        ranksList.Sort(SortRanks);
        Ranks = ranksList.ToArray();
    }

    private static int SortRanks(Rank a, Rank b)
    {
        return a.RequiredScore.CompareTo(b.RequiredScore);
    }
}
