using UnityEngine;
using UnityEngine.AI;

/*
 GoToTrack (infinite loop)
 -------------------------
 - Minimal waypoint follower for bots.
 - Marches child -> child under `targets` and loops forever.
 - Starts at the closest child to reduce weird first turns.
*/

[RequireComponent(typeof(NavMeshAgent))]
public class GoToTrackAI : MonoBehaviour
{
    [Header("Waypoints parent")]
    public Transform targets;                  // parent whose children are the waypoints (e.g., CPContainer)

    [Header("Movement")]
    public float speed = 3.5f;                 // NavMeshAgent speed
    public float arriveDistance = 3f;          // how close counts as "reached"

    private NavMeshAgent agent;
    private int childIndex = 0;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.stoppingDistance = 0f;           // don't stop early
        agent.autoBraking = true;
    }

    void Start()
    {
        if (!targets || targets.childCount == 0)
        {
            Debug.LogWarning($"{name}: GoToTrack has no targets assigned or empty parent.");
            enabled = false;
            return;
        }

        // Start at the closest child for a smooth first leg
        childIndex = FindClosestChildIndex();
        SetDestinationToChild(childIndex);
    }

    void Update()
    {
        if (!targets || targets.childCount == 0) return;

        // Advance when close enough, always modulo count (infinite loop)
        if (!agent.pathPending && agent.remainingDistance <= arriveDistance)
        {
            childIndex = (childIndex + 1) % targets.childCount;
            SetDestinationToChild(childIndex);
        }

        // Keep pushing the current dest so we don't idle near the node
        if (!agent.hasPath) SetDestinationToChild(childIndex);
    }

    // --- helpers ---
    void SetDestinationToChild(int idx)
    {
        if (idx < 0 || idx >= targets.childCount) return;
        var p = targets.GetChild(idx).position;
        agent.destination = p;
    }

    int FindClosestChildIndex()
    {
        int best = 0;
        float bestDist = float.MaxValue;
        Vector3 pos = transform.position;

        for (int i = 0; i < targets.childCount; i++)
        {
            float d = (targets.GetChild(i).position - pos).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = i; }
        }
        return best;
    }
}
