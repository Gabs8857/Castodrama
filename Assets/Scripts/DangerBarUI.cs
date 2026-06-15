using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestionnaire de la barre de danger UI.
/// Le cercle reste toujours visible à 100% et se teinte progressivement
/// du jaune vers le rouge selon le niveau de danger.
/// </summary>
public class DangerBarUI : MonoBehaviour
{
    [SerializeField] private TopDownDanger dangerSystem;
    [SerializeField] private Image dangerBarFill;
    [SerializeField] private Image dangerBarBackground;

    [Header("Couleurs")]
    [SerializeField] private Color lowDangerColor = Color.yellow;
    [SerializeField] private Color highDangerColor = Color.red;

    private void Awake()
    {
        if (dangerBarFill == null)
            dangerBarFill = GetComponentInChildren<Image>();

        if (dangerBarFill != null)
        {
            Debug.Log($"[DangerBarUI] Image liée sur : {dangerBarFill.gameObject.name}");
            if (dangerBarFill.type != Image.Type.Filled)
            {
                Debug.LogWarning("[DangerBarUI] L'image n'était pas en mode 'Filled'. Correction appliquée.");
                dangerBarFill.type = Image.Type.Filled;
                dangerBarFill.fillMethod = Image.FillMethod.Radial360;
            }

            // Le cercle est toujours plein, seule la couleur change
            dangerBarFill.fillAmount = 1f;
        }
        else
        {
            Debug.LogError("[DangerBarUI] Image de remplissage manquante ! La barre ne s'affichera pas.");
        }
        // Le positionnement est géré directement dans l'Inspector.
    }

    private void Start()
    {
        if (dangerSystem == null)
        {
            GameObject player = GameObject.Find("Castor") ?? GameObject.Find("Player");
            if (player != null)
            {
                dangerSystem = player.GetComponent<TopDownDanger>();
                if (dangerSystem != null) Debug.Log("[DangerBarUI] ✓ Système TopDownDanger lié avec succès.");
                else Debug.LogError("[DangerBarUI] ✗ TopDownDanger non trouvé sur le joueur !");
            }
            else
            {
                Debug.LogError("[DangerBarUI] ✗ Objet 'Castor' ou 'Player' introuvable dans la scène.");
            }
        }
    }

    private void Update()
    {
        if (dangerBarFill != null && !dangerBarFill.enabled)
            dangerBarFill.enabled = true;

        if (dangerBarBackground != null && !dangerBarBackground.enabled)
            dangerBarBackground.enabled = true;

        if (dangerSystem != null && dangerBarFill != null)
        {
            bool inZone = dangerSystem.IsInDangerZone;
            float normalized = dangerSystem.NormalizedDanger;

            // Toujours plein, seule la teinte évolue vers le rouge
            dangerBarFill.fillAmount = 1f;
            dangerBarFill.color = Color.Lerp(lowDangerColor, highDangerColor, normalized);

            if (Time.frameCount % 100 == 0 && normalized > 0.001f)
                Debug.Log($"[DangerBarUI] Valeur Danger: {normalized * 100:F1}%");

            if (normalized > 0.01f && Time.frameCount % 30 == 0)
                Debug.Log($"[DangerBarUI] État: {(inZone ? "DANGER" : "REPOS")} | Valeur: {normalized * 100:F1}%");
        }
        else if (Time.frameCount % 200 == 0)
        {
            if (dangerSystem == null) Debug.LogError("[DangerBarUI] dangerSystem (TopDownDanger) est introuvable !");
            if (dangerBarFill == null) Debug.LogError("[DangerBarUI] dangerBarFill (Image) est introuvable !");
        }
    }

    public float GetDangerValue()
    {
        return dangerSystem != null ? dangerSystem.NormalizedDanger : 0f;
    }
}
