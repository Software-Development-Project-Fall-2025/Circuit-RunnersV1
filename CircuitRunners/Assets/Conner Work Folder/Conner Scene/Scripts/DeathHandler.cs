using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathHandler : MonoBehaviour
{
    [Header("Death Effects")]
    public ParticleSystem deathEffectPrimary; // first particle prefab to instantiate on death
    public ParticleSystem deathEffectSecondary; // second particle prefab to instantiate on death

    [Header("Respawn")]
    [Tooltip("Seconds to wait before respawning at last checkpoint")]
    public float respawnDelay = 2f;
    [Tooltip("Health value to set when respawning")]
    public float respawnHealth = 100f;
    [Tooltip("Seconds of invincibility after respawn")]
    public float invincibilityDuration = 3f;

    private CarController carController;
    private CarProgress carProgress;
    private bool hasDied = false;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    // Exposed invincibility flag (read-only externally)
    public bool IsInvincible { get; private set; } = false;

    void Start()
    {
        carController = GetComponentInParent<CarController>();
        if (carController == null)
        {
            Debug.LogWarning("DeathHandler: No CarController found in parents. Assign manually if needed.");
        }
        else
        {
            // set initial spawn transform
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            // try to find CarProgress for checkpoint data
            CarProgress carProgress = carController.GetComponent<CarProgress>();

            carProgress = FindFirstObjectByType<CarProgress>();

            if (carProgress == null)
            {
                Debug.LogError("Car Progress could not be found.");
            }
        }
    }

    void Update()
    {
        if (hasDied) return;
        if (carController == null) return;

        float currentHealth = carController.health;
        // Only trigger when health crosses from >0 to <=0
        if (currentHealth <= 0f)
        {
            hasDied = true;
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        // Play particle effects (instantiate so prefabs can be reused)
        if (deathEffectPrimary != null)
        {
            ParticleSystem ps1 = Instantiate(deathEffectPrimary, transform.position, Quaternion.identity);
            ps1.Play();
            Destroy(ps1.gameObject, ps1.main.duration + ps1.main.startLifetime.constantMax);
        }

        if (deathEffectSecondary != null)
        {
            ParticleSystem ps2 = Instantiate(deathEffectSecondary, transform.position, Quaternion.identity);
            ps2.Play();
            Destroy(ps2.gameObject, ps2.main.duration + ps2.main.startLifetime.constantMax);
        }

        // Disable the CarController so the car can't keep moving
        if (carController != null)
        {
            carController.enabled = false;
        }

        // Start respawn coroutine
        if (respawnDelay >= 0f)
        {
            StartCoroutine(RespawnAfterDelay(respawnDelay));
        }
    }

    private IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        RespawnAtLastCheckpoint();
    }

    private IEnumerator Invincibility(float seconds)
    {
        IsInvincible = true;
        yield return new WaitForSeconds(seconds);
        IsInvincible = false;
    }

    private void RespawnAtLastCheckpoint()
    {
        if (carController == null) return;

        // Determine respawn position: last checkpoint or initial position
        Vector3 spawnPos = initialPosition;
        Quaternion spawnRot = initialRotation;

        if (carProgress != null && carProgress.checkpointManager != null && carProgress.lastCheckpointIndex >= 0)
        {
            var mgr = carProgress.checkpointManager;
            int idx = Mathf.Clamp(carProgress.lastCheckpointIndex, 0, mgr.checkpoints.Length - 1);
            if (mgr.checkpoints != null && mgr.checkpoints.Length > idx && mgr.checkpoints[idx] != null)
            {
                spawnPos = mgr.checkpoints[idx].position;
                spawnRot = mgr.checkpoints[idx].rotation;
            }
        }

        // Move the physics root (sphereRB) and the visual car
        if (carController.sphereRB != null)
        {
            carController.sphereRB.velocity = Vector3.zero;
            carController.sphereRB.angularVelocity = Vector3.zero;
            carController.sphereRB.transform.position = spawnPos;
            carController.transform.position = spawnPos;
            carController.transform.rotation = spawnRot;
        }
        else
        {
            carController.transform.position = spawnPos;
            carController.transform.rotation = spawnRot;
        }

        // Restore health if configured
        carController.health = respawnHealth;

        // Re-enable controller
        carController.enabled = true;

        // Give temporary invincibility after respawn
        if (invincibilityDuration > 0f)
            StartCoroutine(Invincibility(invincibilityDuration));

        // Allow future deaths
        hasDied = false;
    }
}