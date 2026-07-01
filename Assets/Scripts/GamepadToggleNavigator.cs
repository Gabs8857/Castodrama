using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Navigation manette unifiée pour un écran de quiz :
/// le stick droit déplace la sélection parmi les Toggles ET le bouton Valider
/// (qui est traité comme un élément de navigation supplémentaire, en dernière position).
/// A (bouton Sud) agit sur l'élément actuellement sélectionné :
///   - Toggle sélectionné → coche/décoche
///   - Bouton Valider sélectionné → clique Valider
/// 
/// Ça évite qu'un appui sur A déclenche Valider par accident pendant qu'on
/// essaie juste de cocher une proposition.
/// </summary>
[DefaultExecutionOrder(-10)]
public class GamepadToggleNavigator : MonoBehaviour
{
    [Header("Cible")]
    [Tooltip("Le conteneur dont les enfants sont les Toggles à naviguer (ex: choicesContainer)")]
    [SerializeField] private Transform container;

    [Header("Bouton Valider")]
    [Tooltip("Le bouton Valider, traité comme le dernier élément de la navigation")]
    [SerializeField] private Button validateButton;

    [Header("Navigation")]
    [SerializeField] private float stickThreshold = 0.5f;
    [SerializeField] private float moveCooldown = 0.2f;

    [Header("Visuel")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.3f, 1f);
    [Tooltip("Couleur de surbrillance du bouton Valider quand il est sélectionné")]
    [SerializeField] private Color validateHighlightColor = new Color(1f, 0.85f, 0.3f, 1f);

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    // Liste unifiée : toggles + bouton valider à la fin
    private readonly List<Toggle> toggles = new List<Toggle>();
    private readonly List<Image> toggleBackgrounds = new List<Image>();
    private Image validateBackground;
    private Color validateOriginalColor = Color.white;

    private int selectedIndex = 0;
    private float lastMoveTime = -10f;
    private bool stickWasNeutral = true;

    /// <summary>Nombre total d'éléments navigables (toggles + bouton Valider)</summary>
    private int TotalCount => toggles.Count + (validateButton != null ? 1 : 0);

    /// <summary>True si l'élément actuellement sélectionné est le bouton Valider</summary>
    private bool IsValidateSelected => validateButton != null && selectedIndex == toggles.Count;

    private void OnEnable()
    {
        selectedIndex = 0;
        RefreshToggleList();
        CacheValidateBackground();
        HighlightSelected();
    }

    private void Update()
    {
        if (container == null) return;

        // Re-scan si le nombre d'enfants a changé (toggles recréés par BuildToggles)
        if (container.childCount != toggles.Count)
        {
            RefreshToggleList();
            selectedIndex = 0;
            HighlightSelected();
        }

        if (TotalCount == 0) return;

        HandleStickNavigation();
        HandleSelection();
    }

    private void RefreshToggleList()
    {
        toggles.Clear();
        toggleBackgrounds.Clear();

        foreach (Transform child in container)
        {
            Toggle t = child.GetComponent<Toggle>();
            if (t == null) continue;

            toggles.Add(t);
            toggleBackgrounds.Add(t.targetGraphic as Image);
        }

        if (debugLogs)
            Debug.Log($"[GamepadToggleNavigator] {toggles.Count} toggle(s) + {(validateButton != null ? 1 : 0)} bouton Valider");
    }

    private void CacheValidateBackground()
    {
        if (validateButton == null) return;

        validateBackground = validateButton.targetGraphic as Image;
        if (validateBackground != null)
            validateOriginalColor = validateBackground.color;
    }

    private void HandleStickNavigation()
    {
        if (Gamepad.current == null) return;

        Vector2 stick = Gamepad.current.rightStick.ReadValue();
        bool stickIsNeutral = stick.magnitude < stickThreshold;

        if (!stickIsNeutral && stickWasNeutral && Time.time - lastMoveTime > moveCooldown)
        {
            if (Mathf.Abs(stick.y) >= Mathf.Abs(stick.x))
            {
                if (stick.y > 0) MoveSelection(-1);
                else MoveSelection(1);
            }
            else
            {
                if (stick.x > 0) MoveSelection(1);
                else MoveSelection(-1);
            }

            lastMoveTime = Time.time;
        }

        stickWasNeutral = stickIsNeutral;
    }

    private void MoveSelection(int direction)
    {
        int total = TotalCount;
        if (total == 0) return;

        selectedIndex = (selectedIndex + direction + total) % total;
        HighlightSelected();

        if (debugLogs)
            Debug.Log($"[GamepadToggleNavigator] Sélection → index {selectedIndex} ({(IsValidateSelected ? "VALIDER" : "toggle")})");
    }

    private void HandleSelection()
    {
        if (Gamepad.current == null) return;
        if (!Gamepad.current.buttonSouth.wasPressedThisFrame) return; // A

        if (IsValidateSelected)
        {
            if (validateButton != null && validateButton.interactable)
            {
                validateButton.onClick.Invoke();
                if (debugLogs)
                    Debug.Log("[GamepadToggleNavigator] ✓ Valider cliqué via A");
            }
            return;
        }

        if (selectedIndex >= 0 && selectedIndex < toggles.Count)
        {
            Toggle t = toggles[selectedIndex];
            t.isOn = !t.isOn;

            if (debugLogs)
                Debug.Log($"[GamepadToggleNavigator] ✓ Toggle {selectedIndex} → {t.isOn}");
        }
    }

    private void HighlightSelected()
    {
        for (int i = 0; i < toggleBackgrounds.Count; i++)
        {
            if (toggleBackgrounds[i] == null) continue;
            toggleBackgrounds[i].color = (i == selectedIndex) ? highlightColor : Color.white;
        }

        if (validateBackground != null)
        {
            validateBackground.color = IsValidateSelected ? validateHighlightColor : validateOriginalColor;
        }
    }

    /// <summary>
    /// Permet à un autre script (StreamQuestionUI) d'assigner le conteneur dynamiquement.
    /// Réinitialise la sélection sur le premier toggle (pas le bouton Valider).
    /// </summary>
    public void SetContainer(Transform newContainer)
    {
        container = newContainer;
        RefreshToggleList();
        selectedIndex = 0;
        HighlightSelected();
    }
}