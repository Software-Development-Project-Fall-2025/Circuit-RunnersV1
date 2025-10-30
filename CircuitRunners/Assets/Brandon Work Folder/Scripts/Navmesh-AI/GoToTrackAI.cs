using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GoToTrackAI : MonoBehaviour
{
    public CheckpointManager checkpointManager;   // auto-found if left empty
    public CarProgress carProgress;               // auto-found if left empty

    [Header("Tuning")]
    public float nextPointThreshold = 0.75f;      // how close is “reached”
    public float agentSpeed = 8f;

    private NavMeshAgent agent;
    private int targetIndex = -1;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = agentSpeed;
        agent.autoBraking = true;
    }

    void Start()
    {
        if (!checkpointManager) checkpointManager = FindObjectOfType<CheckpointManager>();
        if (!carProgress) carProgress = GetComponent<CarProgress>();
        PickNextTarget(true);
    }

    void Update()
    {
        if (checkpointManager == null || checkpointManager.checkpoints == null || checkpointManager.checkpoints.Length == 0)
            return;

        if (!agent.pathPending && agent.remainingDistance <= nextPointThreshold)
            PickNextTarget(false);
    }

    private void PickNextTarget(bool firstPick)
    {
        int n = checkpointManager.checkpoints.Length;
        if (n == 0) return;

        if (firstPick || carProgress == null || carProgress.lastCheckpointIndex < 0)
            targetIndex = checkpointManager.startIndex;
        else
            targetIndex = (carProgress.lastCheckpointIndex + 1) % n;

        var t = checkpointManager.checkpoints[targetIndex];
        if (t) agent.SetDestination(t.position);
    }
}
