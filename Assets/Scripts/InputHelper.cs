using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Centralise la détection clavier + manette pour toutes les actions du jeu.
/// Gamepad.current.buttonEast/buttonSouth/etc. sont génériques —
/// fonctionnent avec Xbox, PlayStation, Switch Pro automatiquement.
/// 
/// Mapping :
///   E  (Interagir / Avancer dialogue) → B / Croix    (buttonEast)
///   F  (Manger)                       → X / Carré    (buttonWest)
///   G  (Grab / Boue)                  → RB / R1      (rightShoulder)
///   C  (Casser)                       → A / Croix    (buttonSouth)
///   Echap (Pause)                     → - / Select   (selectButton)
///   Navigation quiz/choix             → D-Pad G/D    (dpad)
/// </summary>
public static class InputHelper
{
    public static bool IsGamepadPreferred()
    {
        return Gamepad.current != null;
    }

    // ── Interagir (E / B) — aussi pour avancer dans les dialogues ────────
    public static bool InteractPressed()
    {
        bool keyboard = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        bool gamepad  = Gamepad.current  != null && Gamepad.current.buttonEast.wasPressedThisFrame;
        return keyboard || gamepad;
    }

    public static bool InteractHeld()
    {
        bool keyboard = Keyboard.current != null && Keyboard.current.eKey.isPressed;
        bool gamepad  = Gamepad.current  != null && Gamepad.current.buttonEast.isPressed;
        return keyboard || gamepad;
    }

    /// <summary>
    /// Alias pour "valider/cocher" dans l'UI (quiz, menus) — même bouton qu'Interagir.
    /// </summary>
    public static bool ConfirmPressed() => InteractPressed();

    // ── Manger (F / X) ───────────────────────────────────────────────────
    public static bool EatPressed()
    {
        bool keyboard = Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame;
        bool gamepad  = Gamepad.current  != null && Gamepad.current.buttonWest.wasPressedThisFrame;
        return keyboard || gamepad;
    }

    // ── Grab / Boue (G / RB) ─────────────────────────────────────────────
    public static bool GrabPressed()
    {
        bool keyboard = Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame;
        bool gamepad  = Gamepad.current  != null && Gamepad.current.rightShoulder.wasPressedThisFrame;
        return keyboard || gamepad;
    }

    // ── Casser (C / Y) ───────────────────────────────────────────────────
    public static bool BreakPressed()
    {
        bool keyboard = Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame;
        bool gamepad  = Gamepad.current  != null && Gamepad.current.buttonNorth.wasPressedThisFrame; // Y
        return keyboard || gamepad;
    }

    // ── Pause (Echap / -) ────────────────────────────────────────────────
    public static bool PausePressed()
    {
        bool keyboard = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool gamepad  = Gamepad.current  != null && Gamepad.current.selectButton.wasPressedThisFrame;
        return keyboard || gamepad;
    }

    // ── Avancer dans le dialogue (Espace / A) ────────────────────────────
    /// <summary>
    /// Avance dans un dialogue Ink. Clavier : Espace. Manette : A (buttonSouth).
    /// </summary>
    public static bool SubmitPressed()
    {
        bool keyboard = Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame);
        bool gamepad  = Gamepad.current  != null && Gamepad.current.buttonSouth.wasPressedThisFrame; 
        return keyboard || gamepad;
    }

    public static bool SubmitHeld()
    {
        bool keyboard = Keyboard.current != null && (Keyboard.current.spaceKey.isPressed || Keyboard.current.eKey.isPressed);
        bool gamepad  = Gamepad.current  != null && Gamepad.current.buttonSouth.isPressed;
        return keyboard || gamepad;
    }

    // ── Navigation quiz/choix — Gauche/Droite ────────────────────────────
    /// <summary>
    /// Retourne -1 (gauche), 1 (droite), 0 (rien).
    /// Clavier : Q/D + flèches. Manette : D-Pad G/D.
    /// </summary>
    public static int ChoiceHorizontalPressed()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.qKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
                return -1;
            if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
                return 1;
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.dpad.left.wasPressedThisFrame)  return -1;
            if (Gamepad.current.dpad.right.wasPressedThisFrame) return 1;
        }

        return 0;
    }

    // ── UI Confirm (A / Espace) — pour GamepadToggleNavigator ────────────
    /// <summary>
    /// Valider dans l'ATH/quiz. Manette : A (buttonSouth). Clavier : Espace.
    /// Note : dans le contexte de l'ATH, il n'y a pas d'arbre à casser donc
    /// pas de conflit pratique avec BreakPressed (C/A).
    /// </summary>
    public static bool UIConfirmPressed()
    {
        bool keyboard = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool gamepad  = Gamepad.current  != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
        return keyboard || gamepad;
    }

    public static bool UIConfirmHeld()
    {
        bool keyboard = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
        bool gamepad  = Gamepad.current  != null && Gamepad.current.buttonSouth.isPressed;
        return keyboard || gamepad;
    }

    // ── Debug (F9) ───────────────────────────────────────────────────────
    public static bool DebugTogglePressed()
    {
        return Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame;
    }
}