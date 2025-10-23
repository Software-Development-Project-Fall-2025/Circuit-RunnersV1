using UnityEngine;
using System.Collections;

/*
CheckpointTrigger
-----------------
- Put this on the TRIGGER COLLIDER object under each checkpoint (arch-child).
- It notifies CarProgress when a car passes, and plays a visual cue on the QUAD panel.

Visual cue (fade):
- Assign "optionalVisual" to the QUAD's MeshRenderer (not the arch).
- The Quad's material MUST be URP/Unlit with Surface=Transparent and Render Face=Both.
- This script fades the Quad to alpha=0 (in a color that depends on the checkpoint type) and back.

Color policy:
- Start      => green
- Pre-Finish => blue/cyan
- Others     => yellow
(You can override per checkpoint via 'overrideFadeColor' if you want a custom hue.)
*/

[RequireComponent(typeof(Collider))]
public class CheckpointTrigger : MonoBehaviour
{
    // References
    public CheckpointManager manager;    // drag the CheckPoints parent here
    public int checkpointIndex = 0;      // set index in the Inspector to match order

    // Visuals (Quad fade)
    [Header("Visual cue (QUAD only)")]
    [Tooltip("QUAD MeshRenderer to fade on hit (NOT the arch).")]
    public Renderer optionalVisual;

    [Tooltip("Seconds for fade-out and fade-in each.")]
    public float fadeDuration = 0.25f;

    [Tooltip("Optional hard override. If alpha <= 0, auto-picks Start/PreFinish/Others colors.")]
    public Color overrideFadeColor = new Color(0, 0, 0, 0);

    private bool flashing;   // prevent overlapping fades

    void Reset()
    {
        // Safety: make sure the collider on this object is set up as a trigger
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // Find CarProgress on the object that entered (or its parent)
        var car = other.GetComponentInParent<CarProgress>();
        if (car == null) return;

        if (manager == null) manager = FindObjectOfType<CheckpointManager>();

        // 1) Report progress
        car.OnPassedCheckpoint(checkpointIndex, transform.position);

        // 2) Play Quad fade (if assigned)
        if (optionalVisual != null && !flashing)
            StartCoroutine(FadeQuad(optionalVisual));
    }

    // Smooth fade-out / fade-in on the QUAD's material color/alpha
    private IEnumerator FadeQuad(Renderer r)
    {
        flashing = true;

        var mat = r.material;            // material instance (not shared)
        Color startColor = mat.color;    // original color/alpha, to restore
        float a0 = startColor.a;

        // Pick the visual color for the fade "tint" (alpha will go to 0 in-between)
        Color colorForThis;
        if (overrideFadeColor.a > 0f)
        {
            colorForThis = new Color(overrideFadeColor.r, overrideFadeColor.g, overrideFadeColor.b, 0f);
        }
        else
        {
            if (checkpointIndex == manager.startIndex)             colorForThis = new Color(0f, 1f, 0f, 0f);    // green
            else if (checkpointIndex == manager.preFinishIndex)    colorForThis = new Color(0.2f, 0.7f, 1f, 0f); // blue/cyan
            else                                                   colorForThis = new Color(1f, 0.85f, 0f, 0f);  // yellow
        }

        // --- Fade OUT ---
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float k = t / fadeDuration;
            // tint toward the chosen color and force alpha down to 0
            Color c = Color.Lerp(startColor, colorForThis, k);
            c.a = Mathf.Lerp(a0, 0f, k);
            mat.color = c;
            yield return null;
        }

        // --- Fade IN ---
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float k = t / fadeDuration;
            Color c = Color.Lerp(colorForThis, startColor, k);
            c.a = Mathf.Lerp(0f, a0, k);
            mat.color = c;
            yield return null;
        }

        // restore exactly
        mat.color = startColor;
        flashing = false;
    }
}
