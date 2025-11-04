using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/*
CheckpointManager
-----------------
- Lives on the "CheckPoints" parent GameObject.
- Auto-collects all child transforms as checkpoints (Checkpoint_0, Checkpoint_1, ...).
- Uses lap-rule indices (Start and Pre-Finish for recording).
- Draws gizmos in Scene view so the path is obvious.
- Keeps an optional list of racers (so other systems can query everyone).
- ContextMenu + OnValidate to auto-renumber checkpoints in edit mode.
*/

public class CheckpointManager : MonoBehaviour
{
    [Header("Auto")]
    public Transform[] checkpoints;
    public Transform checkpointsParent;

    [Header("Lap rules")]
    [Tooltip("Which checkpoint is Start/Finish? Usually 0.")]
    public int startIndex = 0;

    [Tooltip("Second-to-last checkpoint. Must be touched sometime before Start to count a lap.")]
    public int preFinishIndex = -1;

    // Racer registry
    private readonly List<CarProgress> racers = new();
    public void RegisterRacer(CarProgress r) { if (!racers.Contains(r)) racers.Add(r); }
    public List<CarProgress> GetRacers() => racers;

    void Start()
    {
        checkpoints = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            checkpoints[i] = transform.GetChild(i);

        preFinishIndex = (checkpoints.Length >= 2) ? checkpoints.Length - 2 : 0;
    }

    [ContextMenu("Renumber Checkpoints")]
    public void RenumberCheckpoints()
    {
        var parent = checkpointsParent ? checkpointsParent : transform;
        int idx = 0;
        foreach (Transform t in parent)
        {
            var trig = t.GetComponentInChildren<CheckpointTrigger>();
            if (trig) trig.checkpointIndex = idx;
            idx++;
        }
        startIndex = 0;
        preFinishIndex = Mathf.Max(0, idx - 2);
        Debug.Log($"[CheckpointManager] Renumbered {idx} checkpoints.");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying) RenumberCheckpoints();
    }
#endif

    void OnDrawGizmos()
    {
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
            if (!t) continue;

            if (i == startIndex) Gizmos.color = Color.green;
            else if (i == preFinishIndex) Gizmos.color = Color.cyan;
            else Gizmos.color = Color.red;
            Gizmos.DrawSphere(t.position, 0.75f);

            int next = (i + 1 < checkpoints.Length) ? i + 1 : -1;
            if (next != -1 && checkpoints[next] != null)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(t.position, checkpoints[next].position);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (checkpoints == null || checkpoints.Length == 0) return;
        Gizmos.color = Color.red;
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (!checkpoints[i]) continue;
            Gizmos.DrawWireSphere(checkpoints[i].position, 1f);
            int next = (i + 1) % checkpoints.Length;
            Gizmos.DrawLine(checkpoints[i].position, checkpoints[next].position);
        }
    }

    // --- Helper for restart (re-enables ring visuals but not CP0 when configured) ---
    public IEnumerable<CheckpointTrigger> AllTriggers()
    {
        foreach (var t in GetComponentsInChildren<CheckpointTrigger>(true))
            yield return t;
    }
}
