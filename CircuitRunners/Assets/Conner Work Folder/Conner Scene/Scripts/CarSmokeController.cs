using UnityEngine;

public class CarSmokeController : MonoBehaviour
{
    [SerializeField] private ParticleSystem smokeParticles;
    [SerializeField] private Gradient smokeColorOverDamage; // Optional (set in Inspector)
    [SerializeField] private float maxEmissionRate = 30f;   // Adjust to fit your effect

    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.EmissionModule emissionModule;

    // healthPercent = 1.0 (100%) = no damage
    // healthPercent = 0.0 (0%) = destroyed
    public void UpdateSmoke(float healthPercent)
    {
        if (smokeParticles == null) return;

        mainModule = smokeParticles.main;
        emissionModule = smokeParticles.emission;

        // --- Determine color ---
        Color smokeColor;
        if (smokeColorOverDamage != null)
        {
            smokeColor = smokeColorOverDamage.Evaluate(1 - healthPercent);
        }
        else
        {
            // Default color curve based on health
            if (healthPercent > 0.8f)
                smokeColor = new Color(1f, 1f, 1f, 0f);       // invisible
            else if (healthPercent > 0.6f)
                smokeColor = new Color(1f, 1f, 1f, 0.3f);     // light white
            else if (healthPercent > 0.4f)
                smokeColor = new Color(0.7f, 0.7f, 0.7f, 0.6f); // grey
            else if (healthPercent > 0.2f)
                smokeColor = new Color(0.3f, 0.3f, 0.3f, 0.8f); // dark grey
            else
                smokeColor = new Color(0f, 0f, 0f, 1f);         // black
        }

        mainModule.startColor = smokeColor;

        // --- Adjust emission ---
        float rate = maxEmissionRate * (1f - healthPercent);
        emissionModule.rateOverTime = rate;        
    }
}