using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Navigation manette pour une liste de boutons empilés verticalement (menu pause, menu principal...).
/// Le stick (gauche ou droit) haut/bas déplace la sélection, A clique le bouton sélectionné.
/// 
/// IMPORTANT : Time.timeScale = 0 pendant la pause — ce script utilise Time.unscaledDeltaTime
/// pour le cooldown afin de continuer à fonctionner même en pause.
/// </summary>
public class GamepadButtonListNavigator : MonoBehaviour
{
    [Header("Boutons (dans l'ordre d'affichage vertical)")]
    [Tooltip("Liste des boutons à naviguer, du plus haut au plus bas")]
    [SerializeField] private List<Button> buttons = new List<Button>();

    [Header("Auto-détection")]
    [Tooltip("Si activé, récupère automatiquement tous les Button enfants de ce GameObject au lieu de la liste manuelle")]
    [SerializeField] private bool autoDetectChildren = false;

    [Header("Navigation")]
    [SerializeField] private float stickThreshold = 0.5f;
    [SerializeField] private float moveCooldown = 0.2f;
    [Tooltip("Utilise le stick gauche en plus du droit (utile en menu pause)")]
    [SerializeField] private bool useLeftStickToo = true;

    [Header("Visuel")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.3f, 1f);

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private readonly List<Image> backgrounds = new List<Image>();
    private readonly List<Color> originalColors = new List<Color>();
    private int selectedIndex = 0;
    private float lastMoveTime = -10f;
    private bool stickWasNeutral = true;

    private void OnEnable()
    {
        RefreshButtons();
    }

    public void RefreshButtons()
    {
        if (autoDetectChildren)
        {
            buttons.Clear();
            buttons.AddRange(GetComponentsInChildren<Button>(true));
        }

        CacheBackgrounds();
        selectedIndex = 0;
        HighlightSelected();

        if (debugLogs)
            Debug.Log($"[GamepadButtonListNavigator] {buttons.Count} bouton(s) à naviguer");
    }

    private void CacheBackgrounds()
    {
        backgrounds.Clear();
        originalColors.Clear();

        foreach (Button b in buttons)
        {
            Image img = b != null ? b.targetGraphic as Image : null;
            backgrounds.Add(img);
            originalColors.Add(img != null ? img.color : Color.white);
        }
    }

    private void Update()
    {
        if (buttons.Count == 0) return;

        HandleStickNavigation();
        HandleSelection();
    }

    private void HandleStickNavigation()
    {
        if (Gamepad.current == null) return;

        Vector2 stick = Gamepad.current.rightStick.ReadValue();
        if (useLeftStickToo && stick.magnitude < stickThreshold)
        {
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            if (leftStick.magnitude > stick.magnitude)
                stick = leftStick;
        }

        bool stickIsNeutral = stick.magnitude < stickThreshold;

        // Time.unscaledTime car le menu pause met Time.timeScale à 0
        if (!stickIsNeutral && stickWasNeutral && Time.unscaledTime - lastMoveTime > moveCooldown)
        {
            if (stick.y > 0) MoveSelection(-1); // haut → précédent
            else if (stick.y < 0) MoveSelection(1); // bas → suivant

            lastMoveTime = Time.unscaledTime;
        }

        stickWasNeutral = stickIsNeutral;
    }

    private void MoveSelection(int direction)
    {
        int count = buttons.Count;
        if (count == 0) return;

        int next = selectedIndex;
        int safety = 0;

        // Skip les boutons inactifs/non-interactables
        do
        {
            next = (next + direction + count) % count;
            safety++;
        }
        while (safety <= count && !IsButtonUsable(buttons[next]));

        selectedIndex = next;
        HighlightSelected();

        if (debugLogs)
            Debug.Log($"[GamepadButtonListNavigator] Sélection → index {selectedIndex}");
    }

    private bool IsButtonUsable(Button b)
    {
        return b != null && b.gameObject.activeInHierarchy && b.interactable;
    }

    private void HandleSelection()
    {
        if (Gamepad.current == null) return;
        if (!Gamepad.current.buttonSouth.wasPressedThisFrame) return; // A

        if (selectedIndex < 0 || selectedIndex >= buttons.Count) return;

        Button b = buttons[selectedIndex];
        if (IsButtonUsable(b))
        {
            b.onClick.Invoke();
            if (debugLogs)
                Debug.Log($"[GamepadButtonListNavigator] ✓ Bouton {selectedIndex} ({b.gameObject.name}) cliqué via A");
        }
    }

    private void HighlightSelected()
    {
        for (int i = 0; i < backgrounds.Count; i++)
        {
            if (backgrounds[i] == null) continue;
            backgrounds[i].color = (i == selectedIndex) ? highlightColor : originalColors[i];
        }
    }

    /// <summary>
    /// Réinitialise la sélection au premier bouton utilisable.
    /// Appeler quand le menu redevient visible (OnEnable le fait déjà automatiquement).
    /// </summary>
    public void ResetSelection()
    {
        selectedIndex = 0;
        HighlightSelected();
    }
}
