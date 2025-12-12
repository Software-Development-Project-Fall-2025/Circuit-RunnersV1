using UnityEngine;
using System.Collections;

public class EngineSoundController : MonoBehaviour
{
    [Header("Car")]
    public Rigidbody rb;

    [Header("Engine Clips")]
    public AudioClip idle;

    public AudioClip low_on;
    public AudioClip low_off;

    public AudioClip med_on;
    public AudioClip med_off;

    public AudioClip high_on;
    public AudioClip high_off;

    public AudioClip maxRPM;

    [Header("Speed Thresholds")]
    public float lowSpeed = 5f;
    public float medSpeed = 15f;
    public float highSpeed = 25f;
    public float maxSpeed = 40f;

    [Header("Crossfade Settings")]
    public float fadeTime = 0.4f;

    private AudioSource srcA;
    private AudioSource srcB;
    private bool usingA = true;

    private string currentState = "";

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();

        // Main source = Source A
        srcA = gameObject.AddComponent<AudioSource>();
        srcB = gameObject.AddComponent<AudioSource>();

        Setup(srcA);
        Setup(srcB);

        PlayState("idle", idle);
    }

    void Setup(AudioSource s)
    {
        s.loop = true;
        s.spatialBlend = 1f;
        s.playOnAwake = false;
        s.volume = 0f;
    }

    private void Update()
    {
        float speed = rb.velocity.magnitude;
        bool accelerating = Input.GetAxis("Vertical") > 0.1f;

        // IDLE
        if (speed < 0.5f)
        {
            SetState("idle", idle);
            return;
        }

        // LOW
        if (speed < lowSpeed)
        {
            SetState(accelerating ? "low_on" : "low_off",
                     accelerating ? low_on : low_off);
            return;
        }

        // MEDIUM
        if (speed < medSpeed)
        {
            SetState(accelerating ? "med_on" : "med_off",
                     accelerating ? med_on : med_off);
            return;
        }

        // HIGH
        if (speed < highSpeed)
        {
            SetState(accelerating ? "high_on" : "high_off",
                     accelerating ? high_on : high_off);
            return;
        }

        // MAX RPM
        SetState("maxRPM", maxRPM);
    }

    private void SetState(string state, AudioClip clip)
    {
        if (state == currentState)
            return;

        currentState = state;
        PlayState(state, clip);
    }

    private void PlayState(string state, AudioClip clip)
    {
        AudioSource fadeIn = usingA ? srcB : srcA;
        AudioSource fadeOut = usingA ? srcA : srcB;

        fadeIn.clip = clip;
        fadeIn.volume = 0f;
        fadeIn.Play();

        StartCoroutine(Crossfade(fadeOut, fadeIn));

        usingA = !usingA;
    }

    private IEnumerator Crossfade(AudioSource from, AudioSource to)
    {
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float n = t / fadeTime;

            from.volume = Mathf.Lerp(1f, 0f, n);
            to.volume = Mathf.Lerp(0f, 1f, n);

            yield return null;
        }

        from.volume = 0f;
        from.Stop();
        to.volume = 1f;
    }
}