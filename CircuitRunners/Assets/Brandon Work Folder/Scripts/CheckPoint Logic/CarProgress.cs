using UnityEngine;

/*
CarProgress
-----------
- Put this on EACH car root (player + every bot).
- Handles:
  * registering with the CheckpointManager
  * lap counting rule: must touch Pre-Finish sometime before Start
  * backwards / out-of-order guard so laps can't be farmed by reversing
  * a continuous "progressScore" used for standings (laps >> cpIndex >> fraction to next)

Notes:
- We keep your debug logs so you can prove it's working during demo.
- "enforceSequentialOrder" can be toggled per car if you ever want a freer rule.
*/

[RequireComponent(typeof(Collider))]  // car needs a collider to hit trigger gates
public class CarProgress : MonoBehaviour
{
    // -------------------------------
    // References
    // -------------------------------
    [Header("Refs")]
    public CheckpointManager checkpointManager;  // drag the CheckPoints parent here in the Inspector

    // -------------------------------
    // Rules
    // -------------------------------
    [Header("Rules")]
    [Tooltip("Require hitting the NEXT checkpoint in sequence to count progress (prevents reverse/skip exploits).")]
    public bool enforceSequentialOrder = true;

    // -------------------------------
    // Runtime (read-only at runtime)
    // -------------------------------
    [Header("Runtime (read-only)")]
    public int lapsCompleted = 0;
    public int lastCheckpointIndex = -1;     // -1 means "haven't touched any"
    public float lastCheckpointTime = 0f;
    public bool touchedPreFinishSinceLastStart = false;

    // For standings: larger = farther along
    public float progressScore = 0f;

    void Start()
    {
        if (checkpointManager == null)
            checkpointManager = FindObjectOfType<CheckpointManager>();
        checkpointManager.RegisterRacer(this);
    }

    // Called by CheckpointTrigger when this car passes a gate.
    public void OnPassedCheckpoint(int cpIndex, Vector3 cpPos)
    {
        // 1) Backwards / out-of-order guard
        if (enforceSequentialOrder && lastCheckpointIndex != -1)
        {
            int total = checkpointManager.checkpoints.Length;
            int expectedNext = (lastCheckpointIndex + 1) % total;
            if (cpIndex != expectedNext)
            {
                Debug.Log($"{name} out-of-order hit (cp={cpIndex}, expected={expectedNext}) → ignored.");
                return;
            }
        }

        // 2) Accept this checkpoint
        Debug.Log($"[{name}] hit CP #{cpIndex} at t={Time.time:0.00}s (lap {lapsCompleted})");
        lastCheckpointIndex = cpIndex;
        lastCheckpointTime = Time.time;

        // 3) Pre-finish bookkeeping
        if (cpIndex == checkpointManager.preFinishIndex)
            touchedPreFinishSinceLastStart = true;

        // 4) Lap counting rule
        if (cpIndex == checkpointManager.startIndex && touchedPreFinishSinceLastStart)
        {
            lapsCompleted++;
            touchedPreFinishSinceLastStart = false; // reset for next lap
            Debug.Log($"[{name}] LAP +1  => {lapsCompleted}");
        }

        // 5) Continuous progress score for standings
        int n = checkpointManager.checkpoints.Length;
        int next = (cpIndex + 1) % n;

        float toNext = Vector3.Distance(transform.position, checkpointManager.checkpoints[next].position);
        float segLen = Mathf.Max(0.1f, Vector3.Distance(
            checkpointManager.checkpoints[cpIndex].position,
            checkpointManager.checkpoints[next].position));

        float frac = 1f - Mathf.Clamp01(toNext / segLen);  // 0..1 in the current segment
        progressScore = lapsCompleted * 10000f + cpIndex * 100f + frac * 100f;
    }

    // Utility used by Restart
    public void ResetProgress()
    {
        lapsCompleted = 0;
        lastCheckpointIndex = -1;
        lastCheckpointTime = 0f;
        touchedPreFinishSinceLastStart = false;
        progressScore = 0f;
    }
}
