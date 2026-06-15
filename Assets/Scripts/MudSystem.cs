using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gère le ramassage de boue en deep swim.
/// Le joueur appuie sur G pour ramasser une boue (max 1 à la fois).
/// </summary>
public class MudSystem : MonoBehaviour
{
    private CharacterAnimator characterAnimator;
    private bool hasMud = false;

    public bool HasMud => hasMud;

    private void Awake()
    {
        characterAnimator = GetComponent<CharacterAnimator>();
        Debug.Log("[MudSystem] ✓ Système de boue initialisé.");
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // Si on appuie sur G mais qu'on n'est pas au fond
        if (Keyboard.current.gKey.wasPressedThisFrame && !characterAnimator.IsSwimmingDeep)
        {
            Debug.Log("[MudSystem] Tentative de ramassage : Impossible car vous n'êtes pas en nage profonde.");
        }

        if (!Keyboard.current.gKey.wasPressedThisFrame) return;
        if (!characterAnimator.IsSwimmingDeep) return;

        if (hasMud)
        {
            Debug.Log("[MudSystem] Tu as déjà une boue !");
            return;
        }

        hasMud = true;
        Debug.Log("[MudSystem] Boue ramassée !");

        // Notifie l'UI
        MudUI mudUI = FindObjectOfType<MudUI>();
        if (mudUI != null) mudUI.UpdateDisplay(hasMud);
    }

    /// <summary>
    /// Utilise la boue (appelé par UnderwaterCrackManager).
    /// </summary>
    public bool UseMud()
    {
        if (!hasMud) return false;
        hasMud = false;

        MudUI mudUI = FindObjectOfType<MudUI>();
        if (mudUI != null) mudUI.UpdateDisplay(hasMud);

        Debug.Log("[MudSystem] Boue utilisée.");
        return true;
    }
}
