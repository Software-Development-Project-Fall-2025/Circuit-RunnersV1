using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
CheckpointTrigger
-----------------
- Put this on the TRIGGER COLLIDER object under each checkpoint (arch-child).
- It notifies CarProgress when a car passes, and plays a visual cue on the QUAD panel.

Visual cue (pulse glow):
- Assign "optionalVisual" to the QUAD's MeshRenderer (not the arch).
- The Quad's material MUST be URP/Unlit with:
    Surface = Transparent, Blending = Additive, Render Face = Both
- This script briefly PULSES brightness (good with a Bloom volume), keeping the quad see-through.

Color policy:
- Start      => green
- Pre-Finish => blue/cyan
- Others     => yellow
(You can override per checkpoint via 'overrideFadeColor' if you want a custom hue.)

Quality-of-life:
- rehitCooldown prevents spammy re-triggers from the same car if it lingers in the gate.
- hideAfterHitSeconds makes top-down views cleaner by hiding arches/rings after they’re triggered.
*/

[RequireComponent(typeof(Collider))]
public class CheckpointTrigger : MonoBehaviour
{
    // -------------------------------
    // References
    // -------------------------------
    public CheckpointManager manager;    // drag the CheckPoints parent here
    public int checkpointIndex = 0;      // set index in the Inspector to match order

    // -------------------------------
    // Visuals (Quad pulse)
    // -------------------------------
    [Header("Visual cue (QUAD only)")]
    [Tooltip("QUAD MeshRenderer to glow on hit (NOT the arch).")]
    public Renderer optionalVisual;

    [Tooltip("Seconds for one pulse (0..peak..0).")]
    public float pulseDuration = 0.25f;

    [Tooltip("Brightness multiplier at pulse peak (works best with Bloom).")]
    public float pulsePeakMultiplier = 3f;

    [Tooltip("Optional hard override. If alpha <= 0, auto-picks Start/PreFinish/Others colors.")]
    public Color overrideFadeColor = new Color(0, 0, 0, 0);

    // -------------------------------
    // Visuals (Hide after hit)
    // -------------------------------
    [Header("Hide after hit (top-down)")]
    [Tooltip("If > 0, disable visual renderers this many seconds after a hit. If == 0, hide immediately. If < 0, never hide.")]
    public float hideAfterHitSeconds = 0.0f;

    [Tooltip("Renderers that make up the arch/ring visuals to hide.")]
    public Renderer[] renderersToHide;

    [Tooltip("If true, visuals re-enable when this object is enabled (useful on race reset).")]
    public bool restoreOnEnable = true;

    void OnEnable()
    {
        if (restoreOnEnable && renderersToHide != null)
            foreach (var r in renderersToHide)
                if (r) r.enabled = true;
    }

    // -------------------------------
    // Debounce (rehit cooldown)
    // -------------------------------
    [Header("Hit debounce")]
    [Tooltip("Ignore repeated hits from the same car within this many seconds.")]
    public float rehitCooldown = 0.5f;

    private readonly Dictionary<CarProgress, float> _lastHitTime = new();
    private bool flashing;   // prevent overlapping pulses

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

        // Debounce: ignore rapid re-hits of the same gate by the same car
        if (_lastHitTime.TryGetValue(car, out var lastT) && Time.time - lastT < rehitCooldown)
            return;
        _lastHitTime[car] = Time.time;

        // 1) Report progress to the car
        car.OnPassedCheckpoint(checkpointIndex, transform.position);

        // 2) Visual pulse (if assigned)
        if (optionalVisual != null && !flashing)
            StartCoroutine(PulseQuad(optionalVisual, pulsePeakMultiplier));

        // 3) Hide after hit (optional)
        if (hideAfterHitSeconds >= 0f)
            StartCoroutine(HideAfterDelay(hideAfterHitSeconds));
        Debug.Log("Checkpoint Reached!" + checkpointIndex);
    }

    // ----------------------------------------------------
    // Pulse the QUAD: 0 -> bright -> 0 (additive glow)
    // - Keeps alpha constant (so it's always see-through)
    // ----------------------------------------------------
    private IEnumerator PulseQuad(Renderer r, float peakMultiplier)
    {
        flashing = true;

        var mat = r.material; // per-instance material
        Color start = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : mat.color;
        float startAlpha = start.a;

        // Pick tint for this checkpoint (alpha kept from start color)
        Color tint;
        if (overrideFadeColor.a > 0f)
            tint = new Color(overrideFadeColor.r, overrideFadeColor.g, overrideFadeColor.b, startAlpha);
        else
        {
            if (checkpointIndex == manager.startIndex) tint = new Color(0f, 1f, 0f, startAlpha);     // green
            else if (checkpointIndex == manager.preFinishIndex) tint = new Color(0.2f, 0.7f, 1f, startAlpha);  // blue/cyan
            else tint = new Color(1f, 0.85f, 0f, startAlpha);   // yellow
        }

        void SetCol(Color c)
        {
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            mat.color = c;
        }

        for (float t = 0; t < pulseDuration; t += Time.deltaTime)
        {
            float k = Mathf.Sin((t / pulseDuration) * Mathf.PI);
            float mul = Mathf.Lerp(1f, peakMultiplier, k);
            Color outCol = Color.Lerp(start, tint, 0.4f) * mul;
            outCol.a = startAlpha;
            SetCol(outCol);
            yield return null;
        }

        SetCol(start);
        flashing = false;
    }

    // -------------------------------
    // Hide visuals after delay
    // -------------------------------
    private IEnumerator HideAfterDelay(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        if (renderersToHide != null)
            foreach (var r in renderersToHide)
                if (r) r.enabled = false;
    }
}
