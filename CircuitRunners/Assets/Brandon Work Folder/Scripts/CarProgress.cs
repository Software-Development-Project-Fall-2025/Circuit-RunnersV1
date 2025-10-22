using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CarProgress : MonoBehaviour
{
    [Header("Refs")]
    public CheckpointManager checkpointManager;  // drag the CheckPoints parent here

    [Header("Runtime (read-only)")]
    public int lapsCompleted = 0;
    public int lastCheckpointIndex = -1;   // -1 until we hit one
    public float lastCheckpointTime = 0f;
    public bool touchedPreFinishSinceLastStart = false;

    // a simple “progress score” to sort positions
    public float progressScore = 0f;

    void Start()
    {
        if (checkpointManager == null)
        {
            checkpointManager = FindObjectOfType<CheckpointManager>();
        }
        checkpointManager.RegisterRacer(this);
    }

    public void OnPassedCheckpoint(int cpIndex, Vector3 cpPos)
    {
        // Debug log every pass
        Debug.Log($"[{name}] hit CP #{cpIndex} at t={Time.time:0.00}s (lap {lapsCompleted})");

        // Update last checkpoint + timestamp
        lastCheckpointIndex = cpIndex;
        lastCheckpointTime = Time.time;

        // Pre-finish bookkeeping: once we pass Start, we expect a pre-finish before next Start
        if (cpIndex == checkpointManager.preFinishIndex)
            touchedPreFinishSinceLastStart = true;

        // Lap counting rule:
        // Count a lap only when we hit Start AND we previously touched the pre-finish.
        if (cpIndex == checkpointManager.startIndex && touchedPreFinishSinceLastStart)
        {
            lapsCompleted++;
            touchedPreFinishSinceLastStart = false; // reset for next lap
            Debug.Log($"[{name}] LAP +1  => {lapsCompleted}");
        }

        // Compute a continuous progress score for standings:
        // score = laps*BIG + cpIndex + fractional distance towards the next cp
        int n = checkpointManager.checkpoints.Length;
        int next = (cpIndex + 1) % n;
        float toNext = Vector3.Distance(transform.position, checkpointManager.checkpoints[next].position);
        float segLen = Mathf.Max(0.1f, Vector3.Distance(checkpointManager.checkpoints[cpIndex].position,
                                                        checkpointManager.checkpoints[next].position));
        float frac = 1f - Mathf.Clamp01(toNext / segLen);

        progressScore = lapsCompleted * 10000f + cpIndex * 100f + frac * 100f;
    }
}
