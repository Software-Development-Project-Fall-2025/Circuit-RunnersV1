using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
CheckpointManager
-----------------
- Lives on the "CheckPoints" parent GameObject.
- Auto-collects all child transforms as checkpoints (Checkpoint_0, Checkpoint_1, ...).
- Uses lap-rule indices (Start and Pre-Finish for recording).
- Draws gizmos in Scene view so the path is obvious.
- Keeps an optional list of racers (so other systems can query everyone).
*/

public class CheckpointManager : MonoBehaviour
{
    // ===============================
    // Auto-collected from children
    // ===============================
    [Header("Auto")]
    public Transform[] checkpoints;   // filled from transform children (order in hierarchy matters)

    // ===============================
    // Lap Rules (simple + robust)
    // ===============================
    [Header("Lap rules")]
    [Tooltip("Which checkpoint is Start/Finish? Usually 0.")]
    public int startIndex = 0;

    [Tooltip("Second-to-last checkpoint. Must be touched sometime before Start to count a lap.\n" +
             "This is auto-set in Start() to (Length - 2).")]
    public int preFinishIndex = -1;

    // ===============================
    // Optional racer registry (will be useful for standings/UI)
    // ===============================
    private readonly List<CarProgress> racers = new();
    public void RegisterRacer(CarProgress r) { if (!racers.Contains(r)) racers.Add(r); }
    public List<CarProgress> GetRacers() => racers;

    // ===============================
    // Unity Lifecycle
    // ===============================
    void Start()
    {
        // Auto-fill from children (keeps workflow simple: drag empties in the scene, no manual array setup)
        checkpoints = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            checkpoints[i] = transform.GetChild(i);
            Debug.Log("Loaded checkpoint: " + checkpoints[i].name);
        }

        // Pre-finish = second-to-last (fallback to 0 if the track is tiny)
        if (checkpoints.Length >= 2) preFinishIndex = checkpoints.Length - 2;
        else preFinishIndex = 0;
    }

    // (kept for parity; currently unused)
    void Update() { }

    // ===============================
    // Gizmos (always-on path hints)
    // ===============================
    void OnDrawGizmos()
    {
        // keep array synced even in edit mode (so moving/adding children updates visuals)
        if (checkpoints == null || checkpoints.Length != transform.childCount)
        {
            checkpoints = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
                checkpoints[i] = transform.GetChild(i);
        }

        if (checkpoints == null || checkpoints.Length == 0) return;

        for (int i = 0; i < checkpoints.Length; i++)
        {
            var t = checkpoints[i];
            if (t == null) continue;

            // Color key for readability while building the track
            if (i == startIndex) Gizmos.color = Color.green;          // Start/Finish
            else if (i == preFinishIndex) Gizmos.color = Color.cyan;  // Pre-Finish
            else Gizmos.color = Color.red;                            // All others

            Gizmos.DrawSphere(t.position, 0.75f);

            // connect to next for a clear loop preview
            int next = (i + 1 < checkpoints.Length) ? i + 1 : -1;
            if (next != -1 && checkpoints[next] != null)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(t.position, checkpoints[next].position);
            }
        }
    }

    // (extra highlight when the parent is selected)
    void OnDrawGizmosSelected()
    {
        if (checkpoints == null || checkpoints.Length == 0) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] != null)
            {
                Gizmos.DrawWireSphere(checkpoints[i].position, 1f);
                int next = (i + 1) % checkpoints.Length;
                Gizmos.DrawLine(checkpoints[i].position, checkpoints[next].position);
            }
        }
    }
}
