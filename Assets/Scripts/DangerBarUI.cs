using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestionnaire de la barre de danger UI.
/// </summary>
public class DangerBarUI : MonoBehaviour
{
    [SerializeField] private TopDownDanger dangerSystem;
    [SerializeField] private Image dangerBarFill;
    [SerializeField] private Image dangerBarBackground;

    [Header("Positioning")]
    [SerializeField] private bool fixedOnScreen = true;
    [SerializeField] private Vector2 anchor = new Vector2(0f, 0f);
    [SerializeField] private Vector2 anchoredPosition = new Vector2(132f, 124f);
    [SerializeField] private Vector2 barSize = new Vector2(96f, 96f);

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Tente de trouver l'image si elle n'est pas assignée
        if (dangerBarFill == null)
            dangerBarFill = GetComponentInChildren<Image>();

        if (dangerBarFill != null) 
        {
            Debug.Log($"[DangerBarUI] Image liée sur : {dangerBarFill.gameObject.name}");
            // Force le mode 'Filled' pour que la barre puisse se remplir
            if (dangerBarFill.type != Image.Type.Filled)
            {
                Debug.LogWarning("[DangerBarUI] L'image n'était pas en mode 'Filled'. Correction appliquée.");
                dangerBarFill.type = Image.Type.Filled;
                dangerBarFill.fillMethod = Image.FillMethod.Radial360;
            }
        }
        else
        {
            Debug.LogError("[DangerBarUI] Image de remplissage manquante ! La barre ne s'affichera pas.");
        }
        
        if (rectTransform != null && fixedOnScreen)
        {
            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = barSize;
        }
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
                Debug.LogError("[DangerBarUI] ✗ Objet 'Castor' ou 'Player' introuvable dans la scène pour lier le danger.");
            }
        }
    }

    private void Update()
    {
        bool shouldBeVisible = !GameState.IsBlockingUI();
        
        if (dangerBarFill != null && dangerBarFill.enabled != shouldBeVisible) 
            dangerBarFill.enabled = shouldBeVisible;

        if (dangerBarBackground != null) dangerBarBackground.enabled = shouldBeVisible;

        if (!shouldBeVisible) return;

        if (dangerSystem != null && dangerBarFill != null)
        {
            bool inZone = dangerSystem.IsInDangerZone;
            float normalized = dangerSystem.NormalizedDanger;
            
            // Mise à jour visuelle
            dangerBarFill.fillAmount = normalized;
            dangerBarFill.color = Color.Lerp(Color.yellow, Color.red, normalized);
            
            // Debug toutes les 100 frames pour vérifier que la valeur bouge bien
            if (Time.frameCount % 100 == 0 && normalized > 0.001f)
                Debug.Log($"[DangerBarUI] Valeur Danger: {normalized * 100:F1}% | FillAmount: {dangerBarFill.fillAmount}");
            
            // Log forcé si le danger est actif pour confirmer le fonctionnement
            if (normalized > 0.01f)
            {
                if (Time.frameCount % 30 == 0) // Log toutes les 30 frames pour ne pas flood
                {
                    Debug.Log($"[DangerBarUI] État: {(inZone ? "DANGER" : "REPOS")} | Valeur: {normalized*100:F1}% | Fill: {dangerBarFill.fillAmount}");
                }
            }
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