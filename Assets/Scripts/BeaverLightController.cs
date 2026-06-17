using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Contrôle la couleur de la lumière du castor en fonction des dangers.
/// Priorités : Zone de danger (orange) > Faim (jaune) > Trop de fuites (bleu) > Sécurité (vert).
/// </summary>
public class BeaverLightController : MonoBehaviour
{
    [Header("🔦 Références")]
    [SerializeField] private Light2D beaverLight; // La lumière à contrôler

    [Header("📊 Systèmes")]
    [SerializeField] private DamManager damManager;
    [SerializeField] private TopDownHunger hungerSystem;
    [SerializeField] private TopDownDanger dangerSystem;

    [Header("🎨 Couleurs")]
    [SerializeField] private Color safeColor = new Color(0f, 1f, 0f); // Vert
    [SerializeField] private Color leakColor = new Color(0f, 0f, 1f); // Bleu
    [SerializeField] private Color hungerColor = new Color(1f, 0.92f, 0.016f); // Jaune vif
    [SerializeField] private Color dangerZoneColor = new Color(1f, 0.65f, 0f); // Orange

    [Header("⚙️ Seuils")]
    [Range(0f, 1f)]
    [Tooltip("Seuil de faim (en %) pour déclencher le JAUNE. Ex: 0.3 = 30% de faim RESTANTE.")]
    [SerializeField] private float hungerThreshold = 0.3f;

    [Range(0f, 1f)]
    [Tooltip("Seuil de fuites (en %) pour déclencher le BLEU. Ex: 0.5 = 50% des fissures actives.")]
    [SerializeField] private float leakThreshold = 0.5f;

    private void Start()
    {
        // Auto-référencement si non assigné
        if (beaverLight == null)
            beaverLight = GetComponent<Light2D>();

        if (damManager == null)
            damManager = FindObjectOfType<DamManager>();

        if (hungerSystem == null)
            hungerSystem = FindObjectOfType<TopDownHunger>();

        if (dangerSystem == null)
            dangerSystem = FindObjectOfType<TopDownDanger>();

        // Vérifications
        if (beaverLight == null)
            Debug.LogError("[BeaverLight] ❌ Aucune Light2D trouvée !");
    }

    private void Update()
    {
        UpdateLightColor();
    }

    private void UpdateLightColor()
    {
        // --- PRIORITÉ 1 : Zone de danger (ORANGE) ---
        if (dangerSystem != null && dangerSystem.IsInDangerZone)
        {
            beaverLight.color = dangerZoneColor;
            return;
        }

        // --- PRIORITÉ 2 : Faim (JAUNE) ---
        if (hungerSystem != null && hungerSystem.NormalizedHunger <= hungerThreshold)
        {
            beaverLight.color = hungerColor;
            return;
        }

        // --- PRIORITÉ 3 : Trop de fuites (BLEU) ---
        if (damManager != null)
        {
            float leakPercentage = (float)damManager.GetCurrentCrackCount() / DamManager.MAX_CRACKS;
            if (leakPercentage >= leakThreshold)
            {
                beaverLight.color = leakColor;
                return;
            }
        }

        // --- PRIORITÉ 4 : Aucun danger (VERT) ---
        beaverLight.color = safeColor;
    }
}