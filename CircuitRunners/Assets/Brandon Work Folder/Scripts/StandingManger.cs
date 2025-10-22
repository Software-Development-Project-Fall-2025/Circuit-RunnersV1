using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class StandingsManager : MonoBehaviour
{
    public CheckpointManager checkpointManager;
    public float refreshHz = 5f;

    private void Start()
    {
        if (checkpointManager == null)
            checkpointManager = FindObjectOfType<CheckpointManager>();
        InvokeRepeating(nameof(RecomputeStandings), 0.2f, 1f / Mathf.Max(1f, refreshHz));
    }

    private void RecomputeStandings()
    {
        List<CarProgress> racers = checkpointManager.GetRacers();
        if (racers == null || racers.Count == 0) return;

        // Sort descending by progressScore
        var ordered = racers.OrderByDescending(r => r.progressScore).ToList();

        for (int i = 0; i < ordered.Count; i++)
        {
            // Here you can update UI per racer if you want
            // e.g., ordered[i].SetPlace(i+1);
            Debug.Log($"POS {i+1}: {ordered[i].name}  score={ordered[i].progressScore:0.0}  lap={ordered[i].lapsCompleted}  lastCP={ordered[i].lastCheckpointIndex}");
        }
    }
}
