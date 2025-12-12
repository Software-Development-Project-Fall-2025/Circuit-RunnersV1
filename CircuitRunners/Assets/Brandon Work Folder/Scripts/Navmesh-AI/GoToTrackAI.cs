using UnityEngine;
using UnityEngine.AI;

/*
 GoToTrackAI (infinite loop)
 -------------------------
 - Minimal waypoint follower for bots.
 - Marches child -> child under `targets` and loops forever.
 - Starts at the closest child to reduce weird first turns.
 - Uses NavMeshAgent for movement (NO Rigidbody forces).
*/

[RequireComponent(typeof(NavMeshAgent))]
public class GoToTrackAI : MonoBehaviour
{
    // -------------------------------
    // Waypoints
    // -------------------------------
    [Header("Waypoints parent")]
    public Transform targets;                  // parent whose children are the waypoints (e.g., CPContainer)

    // -------------------------------
    // Movement
    // -------------------------------
    [Header("Movement")]
    [Tooltip("Movement speed for THIS bot (overrides NavMeshAgent at runtime).")]
    public float speed = 3.5f;

    [Tooltip("How close counts as 'reached' for advancing to the next waypoint.")]
    public float arriveDistance = 3f;

    // -------------------------------
    // Runtime
    // -------------------------------
    private NavMeshAgent agent;
    private int childIndex = 0;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Ensure NavMeshAgent fully controls movement
        agent.updatePosition = true;
        agent.updateRotation = true;

        // IMPORTANT:
        // Speed is set here so each bot can have a unique value
        // without getting reset by the NavMeshAgent defaults.
        agent.speed = speed;

        agent.stoppingDistance = 0f;
        agent.autoBraking = true;
    }

    void Start()
    {
        if (!targets || targets.childCount == 0)
        {
            Debug.LogWarning($"{name}: GoToTrackAI has no targets assigned or empty parent.");
            enabled = false;
            return;
        }

        // Start at the closest waypoint for smoother first turn
        childIndex = FindClosestChildIndex();
        SetDestinationToChild(childIndex);
    }

    void Update()
    {
        if (!agent || !agent.isOnNavMesh) return;
        if (!targets || targets.childCount == 0) return;

        // Advance when close enough, always modulo count (infinite loop)
        if (!agent.pathPending && agent.remainingDistance <= arriveDistance)
        {
            childIndex = (childIndex + 1) % targets.childCount;
            SetDestinationToChild(childIndex);
        }

        // Keep pushing the destination so the agent doesn't idle
        if (!agent.hasPath)
            SetDestinationToChild(childIndex);
    }

    // -------------------------------
    // Helpers
    // -------------------------------
    void SetDestinationToChild(int idx)
    {
        if (idx < 0 || idx >= targets.childCount) return;
        agent.destination = targets.GetChild(idx).position;
    }

    int FindClosestChildIndex()
    {
        int best = 0;
        float bestDist = float.MaxValue;
        Vector3 pos = transform.position;

        for (int i = 0; i < targets.childCount; i++)
        {
            float d = (targets.GetChild(i).position - pos).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }
        return best;
    }
}
