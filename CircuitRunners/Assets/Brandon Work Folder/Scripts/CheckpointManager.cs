using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    // ------------------------------
    // Auto-collected from children
    // ------------------------------
    public Transform[] checkpoints;

    // ------------------------------
    // NEW: Lap rules
    // ------------------------------
    [Header("Lap rules")]
    [Tooltip("Which checkpoint is Start/Finish? Usually 0.")]
    public int startIndex = 0;

    [Tooltip("Second-to-last checkpoint (auto set in Start). Must be touched sometime before Start to count a lap.")]
    public int preFinishIndex = -1;

    // ------------------------------
    // NEW: (Optional) racer registry — lets other systems (standings/UI) find all cars easily
    // ------------------------------
    private readonly List<CarProgress> racers = new();
    public void RegisterRacer(CarProgress r) { if (!racers.Contains(r)) racers.Add(r); }
    public List<CarProgress> GetRacers() => racers;

    // ------------------------------
    // Start is called before the first frame update
    // ------------------------------
    void Start()
    {
        // Auto-fill from children (keeps your workflow simple)
        checkpoints = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            checkpoints[i] = transform.GetChild(i);
            Debug.Log("Loaded checkpoint: " + checkpoints[i].name);   // (kept) debug log
        }

        // NEW: set pre-finish = second-to-last (fallback if very short track)
        if (checkpoints.Length >= 2) preFinishIndex = checkpoints.Length - 2;
        else preFinishIndex = 0;
    }

    // ------------------------------
    // Update is called once per frame (kept for parity; not used)
    // ------------------------------
    void Update() { }

    // ------------------------------
    // (kept) Draw spheres/lines in editor — now with color key
    // ------------------------------
    void OnDrawGizmos()
    {
        // Ensure the array matches children even in edit mode (kept)
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

            // NEW: color key for clarity
            if (i == startIndex) Gizmos.color = Color.green;         // Start/Finish
            else if (i == preFinishIndex) Gizmos.color = Color.cyan; // Pre-finish
            else Gizmos.color = Color.red;                           // Middle checkpoints

            Gizmos.DrawSphere(t.position, 0.75f);

            // line to next (kept)
            int next = (i + 1 < checkpoints.Length) ? i + 1 : -1;
            if (next != -1 && checkpoints[next] != null)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(t.position, checkpoints[next].position);
            }
        }
    }

    // ------------------------------
    // (kept) Highlight when the parent is selected
    // ------------------------------
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
