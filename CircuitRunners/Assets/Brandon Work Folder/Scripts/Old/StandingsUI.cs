using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class StandingsUI : MonoBehaviour
{
    [Header("References")]
    public StandingsManager standingsManager;   // drag your StandingsManager here
    public TextMeshProUGUI listText;            // drag your TMP text object here
    public int maxShown = 8;                    // how many racers to display

    void Start()
    {
        // Auto-find if not set
        if (!standingsManager)
            standingsManager = FindObjectOfType<StandingsManager>();

        // Update regularly (every 0.25 seconds)
        InvokeRepeating(nameof(UpdateStandingsDisplay), 0.25f, 0.25f);
    }

    void UpdateStandingsDisplay()
    {
        if (!standingsManager || standingsManager.checkpointManager == null)
            return;

        // Get all cars from CheckpointManager (you already register them there)
        var racers = standingsManager
            .checkpointManager
            .GetRacers()
            .OrderByDescending(r => r.progressScore)
            .Take(maxShown)
            .Select((r, i) => $"{i + 1}. {r.name}   Laps: {r.lapsCompleted}   CP: {r.lastCheckpointIndex}")
            .ToArray();

        // Update UI
        if (listText != null)
            listText.text = string.Join("\n", racers);
    }
}

