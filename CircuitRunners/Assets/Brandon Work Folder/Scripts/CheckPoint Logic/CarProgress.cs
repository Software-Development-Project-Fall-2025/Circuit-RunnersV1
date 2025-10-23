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
        // Auto-find manager if not assigned (good for quick tests)
        if (checkpointManager == null)
            checkpointManager = FindObjectOfType<CheckpointManager>();

        // Register so the race UI / standings can query everyone if needed
        checkpointManager.RegisterRacer(this);
    }

    /*
    OnPassedCheckpoint
    ------------------
    - Called by CheckpointTrigger when this car passes a gate.
    - cpIndex: the index of the checkpoint we just hit (0..N-1)
    - cpPos:   world position of the checkpoint (not used for logic, useful for debug)
    */
    public void OnPassedCheckpoint(int cpIndex, Vector3 cpPos)
    {
        // 1) Backwards / out-of-order guard (prevents "Start farming" and silly reverse laps)
        if (enforceSequentialOrder && lastCheckpointIndex != -1)
        {
            int total = checkpointManager.checkpoints.Length;
            int expectedNext = (lastCheckpointIndex + 1) % total;

            if (cpIndex != expectedNext)
            {
                // Ignore hits that aren't the next checkpoint in sequence
                Debug.Log($"{name} out-of-order hit (cp={cpIndex}, expected={expectedNext}) → ignored.");
                return;
            }
        }

        // 2) Accept this checkpoint
        Debug.Log($"[{name}] hit CP #{cpIndex} at t={Time.time:0.00}s (lap {lapsCompleted})");
        lastCheckpointIndex = cpIndex;
        lastCheckpointTime = Time.time;

        // 3) Pre-finish bookkeeping:
        // we allow missing intermediate checkpoints (professor's note),
        // BUT to count a lap the car must touch Pre-Finish at some point before Start.
        if (cpIndex == checkpointManager.preFinishIndex)
            touchedPreFinishSinceLastStart = true;

        // 4) Lap counting rule:
        // Count a lap when we hit Start AND we had previously touched the Pre-Finish.
        if (cpIndex == checkpointManager.startIndex && touchedPreFinishSinceLastStart)
        {
            lapsCompleted++;
            touchedPreFinishSinceLastStart = false; // reset for next lap
            Debug.Log($"[{name}] LAP +1  => {lapsCompleted}");
        }

        // 5) Continuous progress score for standings:
        // score = laps*BIG + cpIndex*MED + fractional progress toward next checkpoint
        int n = checkpointManager.checkpoints.Length;
        int next = (cpIndex + 1) % n;

        float toNext = Vector3.Distance(transform.position, checkpointManager.checkpoints[next].position);
        float segLen = Mathf.Max(0.1f, Vector3.Distance(
            checkpointManager.checkpoints[cpIndex].position,
            checkpointManager.checkpoints[next].position));

        float frac = 1f - Mathf.Clamp01(toNext / segLen);  // 0..1 in the current segment

        // Weights: laps dominate >> cp index >> fractional distance (tiebreaker)
        progressScore = lapsCompleted * 10000f + cpIndex * 100f + frac * 100f;
    }
}
