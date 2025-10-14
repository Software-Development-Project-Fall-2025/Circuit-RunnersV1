using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LeaderboardUIManager : MonoBehaviour
{
    public static LeaderboardUIManager Instance { get; private set; }
    [SerializeField] private Text leaderboardText;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateLeaderboard(List<RankData> ranks)
    {
        leaderboardText.text = "🏁 Leaderboard\n";
        foreach (var r in ranks)
        {
            leaderboardText.text += $"Rank {r.rank} - {r.id}\n";
        }
    }
}
