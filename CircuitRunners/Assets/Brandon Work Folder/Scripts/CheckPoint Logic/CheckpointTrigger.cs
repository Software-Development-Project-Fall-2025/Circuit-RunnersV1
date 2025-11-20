using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
CheckpointTrigger (fade + glow)
-------------------------------
- Put this on the TRIGGER COLLIDER object under each checkpoint (arch-child).
- Notifies CarProgress when a car passes.
- Rings: normally near-invisible; briefly glow when hit, then fade back to invisible.
- Start arch (or any renderersToHide): fades out smoothly instead of popping.

Material notes:
- Ring QUAD should use URP/Unlit, Surface=Transparent, Blending=Additive, RenderFace=Both.
- Script uses _BaseColor when present; falls back to .color otherwise.
*/

[RequireComponent(typeof(Collider))]
public class CheckpointTrigger : MonoBehaviour
{
    // -------------------------------
    // References
    // -------------------------------
    public CheckpointManager manager;
    public int checkpointIndex = 0;

    // -------------------------------
    // Ring visual (glow)
    // -------------------------------
    [Header("Ring Visual (QUAD only)")]
    [Tooltip("Assign the ring QUAD MeshRenderer here (not the arch).")]
    public Renderer optionalVisual;

    [Tooltip("Default alpha for rings when idle (≈ invisible for top-down).")]
    [Range(0f, 1f)] public float ringIdleAlpha = 0.02f;

    [Tooltip("How bright the glow gets at peak (additive look, good with Bloom).")]
    public float ringGlowMultiplier = 3.0f;

    [Tooltip("How long the glow flashes before fading back.")]
    public float ringGlowSeconds = 0.25f;

    [Tooltip("Fade time from glow back to invisible.")]
    public float ringFadeBackSeconds = 0.35f;

    // -------------------------------
    // Objects to hide (arch, tall props)
    // -------------------------------
    [Header("Hide After Hit (Arch / Tall Meshes)")]
    [Tooltip("Renderers that should fade out and disable after a hit (e.g., the start arch mesh).")]
    public Renderer[] renderersToHide;

    [Tooltip("Seconds to wait before starting the fade (set 0 for immediate).")]
    public float hideAfterHitSeconds = 0f;

    [Tooltip("Seconds the arch fade should take.")]
    public float archFadeSeconds = 0.6f;

    [Tooltip("If true, visuals re-enable when this object is enabled (useful on race reset).")]
    public bool restoreOnEnable = true;

    // CP0 one-time permanent hide when the *player* first starts the race
    [Header("Special: CP0 Start Arch")]
    [Tooltip("Hide permanently the first time the *Player* crosses CP0.")]
    public bool permanentHideWhenPlayerHitsStart = false;

    [Tooltip("Tag used to detect the Player for the permanent hide at CP0.")]
    public string playerTag = "Car";

    private bool _permaHidden = false;

    // -------------------------------
    // Debounce (rehit cooldown)
    // -------------------------------
    [Header("Hit Debounce")]
    [Tooltip("Time window to ignore repeated collider entries by the same car.")]
    public float rehitCooldown = 0.5f;

    private readonly Dictionary<CarProgress, float> _lastHitTime = new();

    // -------------------------------
    // Setup
    // -------------------------------
    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void OnEnable()
    {
        // Re-enable arch/tall meshes unless permanently hidden
        if (restoreOnEnable && !_permaHidden && renderersToHide != null)
        {
            foreach (var r in renderersToHide) if (r) r.enabled = true;
        }

        // Make ring near-invisible by default
        if (optionalVisual != null)
        {
            var m = optionalVisual.material;
            SetColorAlpha(m, ringIdleAlpha);
        }
    }

    // -------------------------------
    // Trigger
    // -------------------------------
    void OnTriggerEnter(Collider other)
    {
        CarProgress car;
        
        // Debug.Log(other.name); (Car component name Debug)
        if(other.tag == "Car")
        {
           car = other.GetComponent<CarProgress>();
        } else 
        {
            car = other.GetComponentInParent<CarProgress>();
        }

        // Debug.Log(car.name); (Car name debug)

        if (!car) return;
        if (!manager) manager = FindObjectOfType<CheckpointManager>();


        // Debounce per car
        if (_lastHitTime.TryGetValue(car, out var lastT) && Time.time - lastT < rehitCooldown) return;
        _lastHitTime[car] = Time.time;
        Debug.Log("HIT!!!");

        // 1) Report progress
        car.OnPassedCheckpoint(checkpointIndex, transform.position);

        // 2) Ring: quick glow then fade back to invisible
        if (optionalVisual != null)
            StartCoroutine(GlowThenFade(optionalVisual.material, ringGlowMultiplier, ringGlowSeconds, ringFadeBackSeconds, ringIdleAlpha));

        // 3) Arch / tall meshes: fade then disable
        if (renderersToHide != null && renderersToHide.Length > 0)
            StartCoroutine(FadeAfterDelay(renderersToHide, hideAfterHitSeconds, archFadeSeconds));

        // 4) CP0 one-time permanent hide for Player
        if (!_permaHidden
            && permanentHideWhenPlayerHitsStart
            && manager != null
            && checkpointIndex == manager.startIndex
            && other.CompareTag(playerTag))
        {
            _permaHidden = true;
            restoreOnEnable = false; // keep it gone on restart
        }

        Debug.Log("Checkpoint Reached!" + checkpointIndex);
    }

    // -------------------------------
    // Coroutines
    // -------------------------------

    // Fades a set of renderers to alpha 0 then disables them.
    private IEnumerator FadeAfterDelay(Renderer[] rends, float delay, float fadeSeconds)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        if (rends == null || rends.Length == 0) yield break;

        // Capture materials + start colors
        var mats = new List<Material>(rends.Length);
        var starts = new List<Color>(rends.Length);
        foreach (var r in rends)
        {
            if (!r) continue;
            var m = r.material; // instance
            mats.Add(m);
            starts.Add(GetColor(m));
        }

        // Fade
        for (float t = 0; t < fadeSeconds; t += Time.deltaTime)
        {
            float aMul = 1f - Mathf.Clamp01(t / fadeSeconds);
            for (int i = 0; i < mats.Count; i++)
            {
                var c = starts[i];
                c.a *= aMul;
                SetColor(mats[i], c);
            }
            yield return null;
        }

        // End state: fully transparent + disable renderers
        for (int i = 0; i < mats.Count; i++)
        {
            var c = starts[i]; c.a = 0f;
            SetColor(mats[i], c);
        }
        foreach (var r in rends) if (r) r.enabled = false;
    }

    // Makes the ring glow briefly, then fades its alpha back to idleAlpha.
    private IEnumerator GlowThenFade(Material m, float peakMul, float glowSecs, float fadeBackSecs, float targetAlpha)
    {
        Color baseCol = GetColor(m);
        baseCol.a = targetAlpha; // ensure baseline is near-invisible
        SetColor(m, baseCol);

        // Glow up/down
        for (float t = 0; t < glowSecs; t += Time.deltaTime)
        {
            float k = Mathf.Sin((t / glowSecs) * Mathf.PI); // 0→1→0
            float mul = Mathf.Lerp(1f, peakMul, k);
            var c = baseCol * mul;
            c.a = Mathf.Clamp01(targetAlpha + 0.08f);       // slight visibility during glow
            SetColor(m, c);
            yield return null;
        }

        // Fade back to invisible alpha
        Color start = GetColor(m);
        for (float t = 0; t < fadeBackSecs; t += Time.deltaTime)
        {
            float a = Mathf.Lerp(start.a, targetAlpha, t / fadeBackSecs);
            var c = start; c.a = a;
            SetColor(m, c);
            yield return null;
        }

        // Lock at idle
        baseCol.a = targetAlpha;
        SetColor(m, baseCol);
    }

    // -------------------------------
    // Material helpers
    // -------------------------------
    static Color GetColor(Material m)
    {
        return m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : m.color;
    }

    static void SetColor(Material m, Color c)
    {
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        m.color = c;
    }

    static void SetColorAlpha(Material m, float a)
    {
        var c = GetColor(m); c.a = a; SetColor(m, c);
    }
}
