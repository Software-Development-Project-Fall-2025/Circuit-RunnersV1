using System.Linq;
using System.Text;
using UnityEngine;
using TMPro;

/*
StandingManger
--------------
- Lives on your RaceHud Canvas (or any GameObject).
- Uses the existing StandingUI (TMP) to show standings in the UPPER-RIGHT.
- Pulls racers from the single CheckpointManager (CPContainer).
*/

public class StandingsManager : MonoBehaviour
{
    [Header("Refs")]
    public CheckpointManager checkpointManager;   // CPContainer (CheckpointManager)
    public TMP_Text standingUI;                   // your StandingUI TMP text

    [Header("Update")]
    [Range(1, 30)] public int refreshHz = 5;

    float _nextRefresh;

    void Start()
    {
        if (!checkpointManager) checkpointManager = FindObjectOfType<CheckpointManager>();

        if (standingUI)
        {
            var rt = standingUI.GetComponent<RectTransform>();
            // force to upper-right every time (no conditional)
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-20f, -20f); // 20px in from top-right
            standingUI.alignment = TMPro.TextAlignmentOptions.TopRight;
        }
    }


    void Update()
    {
        if (!standingUI || !checkpointManager) return;

        // throttle refresh
        if (Time.time < _nextRefresh)
            return;
        _nextRefresh = Time.time + (1f / Mathf.Max(1, refreshHz));

        var racers = checkpointManager.GetRacers();
        if (racers == null || racers.Count == 0) return;

        var ordered = racers.OrderByDescending(r => r.progressScore).ToList();

        var sb = new StringBuilder();
        for (int i = 0; i < ordered.Count; i++)
        {
            var r = ordered[i];
            sb.Append(i + 1).Append(". ").Append(r.name)
              .Append("   Laps: ").Append(r.lapsCompleted)
              .Append("   CP: ").Append(r.lastCheckpointIndex < 0 ? "—" : r.lastCheckpointIndex.ToString())
              .Append('\n');
        }

        standingUI.text = sb.ToString();
    }
}
