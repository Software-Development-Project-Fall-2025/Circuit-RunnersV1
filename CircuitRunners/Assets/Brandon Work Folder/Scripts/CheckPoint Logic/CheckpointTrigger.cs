using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
CheckpointTrigger (fade + glow)
-------------------------------
- Trigger-based + distance-based checkpoint detection.
- Distance fallback is REQUIRED for NavMeshAgent-driven cars.
- Keeps all existing visuals, fade, debounce, and CP0 logic intact.
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
    // Detection (NavMesh-safe fallback)
    // -------------------------------
    [Header("Distance Fallback")]
    [Tooltip("Radius used if trigger events fail (NavMeshAgent safe).")]
    public float distanceTriggerRadius = 2.75f;

    // -------------------------------
    // Ring visual (glow)
    // -------------------------------
    [Header("Ring Visual (QUAD only)")]
    public Renderer optionalVisual;

    [Range(0f, 1f)] public float ringIdleAlpha = 0.02f;
    public float ringGlowMultiplier = 3.0f;
    public float ringGlowSeconds = 0.25f;
    public float ringFadeBackSeconds = 0.35f;

    // -------------------------------
    // Objects to hide (arch, tall props)
    // -------------------------------
    [Header("Hide After Hit (Arch / Tall Meshes)")]
    public Renderer[] renderersToHide;
    public float hideAfterHitSeconds = 0f;
    public float archFadeSeconds = 0.6f;
    public bool restoreOnEnable = true;

    // CP0 one-time permanent hide when the *player* first starts the race
    [Header("Special: CP0 Start Arch")]
    public bool permanentHideWhenPlayerHitsStart = false;
    public string playerTag = "Car";

    private bool _permaHidden = false;

    // -------------------------------
    // Debounce (shared)
    // -------------------------------
    [Header("Hit Debounce")]
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
        if (restoreOnEnable && !_permaHidden && renderersToHide != null)
        {
            foreach (var r in renderersToHide)
                if (r) r.enabled = true;
        }

        if (optionalVisual != null)
        {
            var m = optionalVisual.material;
            SetColorAlpha(m, ringIdleAlpha);
        }
    }

    // -------------------------------
    // Trigger-based detection (physics)
    // -------------------------------
    void OnTriggerEnter(Collider other)
    {
        TryRegisterHit(other.GetComponentInParent<CarProgress>(), other);
    }

    // -------------------------------
    // Distance-based detection (NavMesh fallback)
    // -------------------------------
    void Update()
    {
        if (!manager) manager = FindObjectOfType<CheckpointManager>();
        if (!manager) return;

        foreach (var car in manager.GetRacers())
        {
            float d = Vector3.Distance(car.transform.position, transform.position);
            if (d > distanceTriggerRadius) continue;

            TryRegisterHit(car, null);
        }
    }

    // -------------------------------
    // Shared hit handler
    // -------------------------------
    void TryRegisterHit(CarProgress car, Collider other)
    {
        if (!car) return;

        // Debounce
        if (_lastHitTime.TryGetValue(car, out var lastT) &&
            Time.time - lastT < rehitCooldown)
            return;

        _lastHitTime[car] = Time.time;

        Debug.Log($"[CheckpointTrigger] HIT: {car.name} -> CP {checkpointIndex}");

        // 1) Progress
        car.OnPassedCheckpoint(checkpointIndex, transform.position);

        // 2) Ring visual
        if (optionalVisual != null)
            StartCoroutine(GlowThenFade(
                optionalVisual.material,
                ringGlowMultiplier,
                ringGlowSeconds,
                ringFadeBackSeconds,
                ringIdleAlpha));

        // 3) Arch fade
        if (renderersToHide != null && renderersToHide.Length > 0)
            StartCoroutine(FadeAfterDelay(
                renderersToHide,
                hideAfterHitSeconds,
                archFadeSeconds));

        // 4) CP0 permanent hide (player only)
        if (!_permaHidden &&
            permanentHideWhenPlayerHitsStart &&
            manager != null &&
            checkpointIndex == manager.startIndex &&
            other != null &&
            other.CompareTag(playerTag))
        {
            _permaHidden = true;
            restoreOnEnable = false;
        }
    }

    // -------------------------------
    // Coroutines
    // -------------------------------
    private IEnumerator FadeAfterDelay(Renderer[] rends, float delay, float fadeSeconds)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        if (rends == null || rends.Length == 0) yield break;

        var mats = new List<Material>();
        var starts = new List<Color>();

        foreach (var r in rends)
        {
            if (!r) continue;
            mats.Add(r.material);
            starts.Add(GetColor(r.material));
        }

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

        foreach (var r in rends)
            if (r) r.enabled = false;
    }

    private IEnumerator GlowThenFade(Material m, float peakMul, float glowSecs, float fadeBackSecs, float targetAlpha)
    {
        Color baseCol = GetColor(m);
        baseCol.a = targetAlpha;
        SetColor(m, baseCol);

        for (float t = 0; t < glowSecs; t += Time.deltaTime)
        {
            float k = Mathf.Sin((t / glowSecs) * Mathf.PI);
            var c = baseCol * Mathf.Lerp(1f, peakMul, k);
            c.a = Mathf.Clamp01(targetAlpha + 0.08f);
            SetColor(m, c);
            yield return null;
        }

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
