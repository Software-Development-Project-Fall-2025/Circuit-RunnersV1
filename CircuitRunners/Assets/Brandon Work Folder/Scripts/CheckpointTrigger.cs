using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class CheckpointTrigger : MonoBehaviour
{
    public CheckpointManager manager;
    public int checkpointIndex;
    public Renderer optionalVisual; // assign if you want a flash

    private void Reset()
    {
        // Ensure trigger
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // find CarProgress on the thing that entered (or its parent)
        var car = other.GetComponentInParent<CarProgress>();
        if (car == null) return;

        if (manager == null) manager = FindObjectOfType<CheckpointManager>();
        car.OnPassedCheckpoint(checkpointIndex, transform.position);

        // quick visual cue (fade alpha down/up)
        if (optionalVisual != null)
            StartCoroutine(Flash(optionalVisual));
    }

    private IEnumerator Flash(Renderer r)
    {
        var mat = r.material; // instance
        Color c = mat.color;
        float a0 = c.a;

        // fade down
        for (float t = 0; t < .15f; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(a0, 0.25f, t / .15f);
            mat.color = c;
            yield return null;
        }
        // hold a moment
        yield return new WaitForSeconds(.05f);
        // fade back
        for (float t = 0; t < .15f; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(0.25f, a0, t / .15f);
            mat.color = c;
            yield return null;
        }
    }
}
