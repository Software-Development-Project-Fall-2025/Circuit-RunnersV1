using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public Transform[] checkpoints;

    // Start is called before the first frame update
    void Start()
    {
        checkpoints = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            checkpoints[i] = transform.GetChild(i);
            Debug.Log("Loaded checkpoint: " + checkpoints[i].name);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDrawGizmos()
    {
    // Ensure the array matches children even in edit mode
    if (checkpoints == null || checkpoints.Length != transform.childCount)
        {
        checkpoints = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            checkpoints[i] = transform.GetChild(i);
        }

    // Draw spheres and lines
    Gizmos.color = Color.green;
    for (int i = 0; i < checkpoints.Length; i++)
        {
        var t = checkpoints[i];
        if (t == null) continue;

        Gizmos.DrawSphere(t.position, 0.75f);

        // line to next checkpoint
        int next = (i + 1 < checkpoints.Length) ? i + 1 : -1;
        if (next != -1 && checkpoints[next] != null)
            Gizmos.DrawLine(t.position, checkpoints[next].position);
        } 
    }
    void OnDrawGizmosSelected()
    {
    Gizmos.color = Color.red;
    for (int i = 0; i < checkpoints.Length; i++)
    {
        if (checkpoints[i] != null)
        {
            Gizmos.DrawWireSphere(checkpoints[i].position, 1f);

            // Connect to next
            int next = (i + 1) % checkpoints.Length;
            Gizmos.DrawLine(checkpoints[i].position, checkpoints[next].position);
            }
        }
    }

}


