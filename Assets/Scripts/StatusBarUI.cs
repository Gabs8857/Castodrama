using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestionnaire unifié des barres d'état UI (Faim + Danger)
/// Remplace TopDownHungerBarUI.cs et TopDownDangerBarUI.cs
/// 
/// Gère:
/// - Barre de faim circulaire (haut centre)
/// - Barre de danger circulaire (bas gauche)
/// - Mise à jour en temps réel selon les systèmes TopDownHunger et TopDownDanger
/// </summary>
public class StatusBarUI : MonoBehaviour
{
    // ==================== FAIM ====================
    [SerializeField]
    private TopDownHunger hungerSystem;

    [SerializeField]
    private Image hungerBarFill;

    [SerializeField]
    private Image hungerBarBackground;

    [SerializeField]
    private Sprite hungerBackgroundSprite;

    [SerializeField]
    private Image hungerCenterIcon;

    [SerializeField]
    private Image hungerRing;  // Cercle autour de la barre

    [SerializeField]
    private bool hungerFixedOnScreen = true;

    [SerializeField]
    private RectTransform hungerFixedCanvasPoint;

    [SerializeField]
    private Vector2 hungerFixedAnchoredPosition = new Vector2(0f, 84f);

    [SerializeField]
    private Vector2 hungerBarSize = new Vector2(128f, 128f);

    // ==================== DANGER ====================
    [SerializeField]
    private TopDownDanger dangerSystem;

    [SerializeField]
    private Image dangerBarFill;

    [SerializeField]
    private Image dangerBarBackground;

    [SerializeField]
    private Vector2 dangerAnchoredPosition = new Vector2(132f, 124f);

    [SerializeField]
    private Vector2 dangerBarSize = new Vector2(96f, 96f);

    [SerializeField]
    private float dangerRingThicknessFactor = 0.34f;

    private RectTransform hungerRectTransform;
    private RectTransform dangerRectTransform;
    private RectTransform hungerRingRectTransform;

    private void Awake()
    {
        hungerRectTransform = hungerBarFill?.GetComponent<RectTransform>();
        dangerRectTransform = dangerBarFill?.GetComponent<RectTransform>();
        hungerRingRectTransform = hungerRing?.GetComponent<RectTransform>();
    }

    private void Start()
    {
        // Trouve les systèmes s'ils ne sont pas assignés
        if (hungerSystem == null)
        {
            GameObject playerObject = GameObject.Find("Player");
            if (playerObject != null)
            {
                hungerSystem = playerObject.GetComponent<TopDownHunger>();
            }
        }

        if (dangerSystem == null)
        {
            GameObject playerObject = GameObject.Find("Player");
            if (playerObject != null)
            {
                dangerSystem = playerObject.GetComponent<TopDownDanger>();
            }
        }

        // Crée automatiquement le ring s'il n'existe pas
        if (hungerRing == null)
        {
            hungerRing = CreateRingAroundBar(gameObject, hungerBarSize);
        }

        // Configure les positions fixes si activé
        if (hungerFixedOnScreen && hungerRectTransform != null && hungerFixedCanvasPoint != null)
        {
            hungerRectTransform.anchoredPosition = hungerFixedAnchoredPosition;
            hungerRectTransform.sizeDelta = hungerBarSize;
        }

        // Configure le ring autour de la HungerBar
        if (hungerRingRectTransform != null)
        {
            hungerRingRectTransform.anchoredPosition = Vector2.zero;
            // Le ring est légèrement plus grand que la barre
            hungerRingRectTransform.sizeDelta = hungerBarSize * 1.2f;
            Debug.Log("[StatusBarUI] Ring configuré à la position: " + hungerRingRectTransform.anchoredPosition);
        }

        if (dangerRectTransform != null)
        {
            dangerRectTransform.anchoredPosition = dangerAnchoredPosition;
            dangerRectTransform.sizeDelta = dangerBarSize;
        }
    }

    /// <summary>
    /// Crée automatiquement un ring/cercle autour de la barre
    /// </summary>
    private Image CreateRingAroundBar(GameObject parentBar, Vector2 barSize)
    {
        // Cherche d'abord si un ring existe déjà
        Transform existingRing = parentBar.transform.Find("Ring");
        if (existingRing != null)
        {
            Image existingImage = existingRing.GetComponent<Image>();
            if (existingImage != null)
            {
                hungerRingRectTransform = existingRing.GetComponent<RectTransform>();
                return existingImage;
            }
        }

        // Crée le GameObject Ring
        GameObject ringObject = new GameObject("Ring");
        ringObject.transform.SetParent(parentBar.transform, false);

        RectTransform ringRect = ringObject.AddComponent<RectTransform>();
        ringRect.anchoredPosition = Vector2.zero;
        ringRect.sizeDelta = barSize * 1.2f;

        // Crée l'Image du ring - avec contour/border visible
        Image ringImage = ringObject.AddComponent<Image>();
        ringImage.sprite = CreateRingSprite();
        ringImage.color = new Color(0.7f, 0.7f, 0.7f, 0.8f); // Gris plus visible
        ringImage.type = Image.Type.Simple;

        // Force le ring à être derrière la barre de remplissage
        CanvasGroup canvasGroup = ringObject.AddComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        hungerRingRectTransform = ringRect;
        Debug.Log("[StatusBarUI] Ring créé autour de HungerBar avec size: " + ringRect.sizeDelta);
        return ringImage;
    }

    /// <summary>
    /// Crée un sprite circulaire avec un contour/ring visible
    /// </summary>
    private Sprite CreateRingSprite()
    {
        // Pour maintenant, utilise la texture blanche
        // Tu peux remplacer ça avec une vraie texture circulaire si tu en as une
        return Sprite.Create(Texture2D.whiteTexture, 
            new Rect(0f, 0f, 1f, 1f), 
            new Vector2(0.5f, 0.5f), 
            1f);
    }

    private void Update()
    {
        // Mets à jour la barre de faim
        if (hungerSystem != null && hungerBarFill != null)
        {
            float hungerFillAmount = hungerSystem.CurrentHunger / hungerSystem.MaxHunger;
            hungerBarFill.fillAmount = hungerFillAmount;
        }

        // Mets à jour la barre de danger
        if (dangerSystem != null && dangerBarFill != null)
        {
            float dangerFillAmount = dangerSystem.CurrentDanger / dangerSystem.MaxDanger;
            dangerBarFill.fillAmount = dangerFillAmount;
        }
    }

    /// <summary>
    /// Retourne la valeur de faim actuelle
    /// </summary>
    public float GetHungerValue()
    {
        if (hungerSystem != null)
        {
            return hungerSystem.CurrentHunger / hungerSystem.MaxHunger;
        }
        return 0f;
    }

    /// <summary>
    /// Retourne la valeur de danger actuelle
    /// </summary>
    public float GetDangerValue()
    {
        if (dangerSystem != null)
        {
            return dangerSystem.CurrentDanger / dangerSystem.MaxDanger;
        }
        return 0f;
    }
}
