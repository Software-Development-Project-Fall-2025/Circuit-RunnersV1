using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GoToTrack : MonoBehaviour
{
    public Transform targets; // The target to move towards
    public float speed = 3.5f; // The speed of the agent
    private NavMeshAgent agent;

    private int childIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.destination = targets.GetChild(childIndex).position;
    }

    // Update is called once per frame
    void Update()
    {
        if (agent.remainingDistance < 0.5f)
        {
            childIndex++;
            if (childIndex >= targets.childCount)
            {
                childIndex = 0; // Loop back to the first target
            }
            agent.destination = targets.GetChild(childIndex).position;
        }
    }
        
}

