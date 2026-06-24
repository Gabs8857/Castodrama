using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Centralise la détection clavier + manette pour toutes les actions du jeu.
/// Gamepad.current.buttonEast/buttonSouth/etc. sont génériques : fonctionnent
/// avec Xbox, PlayStation, Switch Pro et autres manettes automatiquement
/// (le New Input System remappe les boutons physiques vers ces noms communs).
/// 
/// Mapping :
///   E (Interagir)      → B / Croix (buttonEast)
///   F (Manger)         → X / Carré (buttonWest)
///   G (Grab/Boue)       → RB (rightShoulder)
///   C (Casser)         → A / Croix (buttonSouth)
///   Espace (Soumettre) → Y / Triangle (buttonNorth) - pour dialogues, menus
///   Espace (UI/ATH)    → A / Croix (buttonSouth) - pour valider dans l'ATH
///   Echap (Pause)      → Start / Select (- sur Switch / View sur Xbox)
///   Navigation Quiz    → D-Pad Gauche/Droite + Joystick Droit (horizontal)
///   Valider/Interagir UI → E / B (même bouton que l'interaction monde)
/// 
/// Usage : remplace
///   if (Keyboard.current.eKey.wasPressedThisFrame)
/// par
///   if (InputHelper.InteractPressed())
/// 
///   if (Keyboard.current.spaceKey.wasPressedThisFrame)
/// par
///   if (InputHelper.SubmitPressed()) OU InputHelper.UIConfirmPressed()
/// 
///   if (Gamepad.current.leftStick.ReadValue().x ...) pour le mouvement
///   Utilisez InputHelper.ChoiceHorizontalPressed() pour la navigation UI
/// </summary>
public static class InputHelper
{
    // ── Interagir (E / B-Croix) — aussi utilisé pour valider/cocher dans l'ATH ──
    public static bool InteractPressed()
    {
        bool keyboard = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        bool gamepad = Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;
        return keyboard || gamepad;
    }

    public static bool InteractHeld()
    {
        bool keyboard = Keyboard.current != null && Keyboard.current.eKey.isPressed;
        bool gamepad = Gamepad.current != null && Gamepad.current.buttonEast.isPressed;
        return keyboard || gamepad;
    }

    /// <summary>
    /// Alias explicite pour "valider/cocher" dans l'UI (quiz, menus).
    /// Même bouton que InteractPressed mais nommé pour la clarté du contexte UI.
    /// </summary>
    public static bool ConfirmPressed() => InteractPressed();

    // ── Manger (F / X-Carré) ─────────────────────────────────────────────
    public static bool EatPressed()
    {
        bool keyboard = Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame;
        bool gamepad = Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame;
        return keyboard || gamepad;
    }

    // ── Grab / Boue (G / RB) ─────────────────────────────────────────────
    public static bool GrabPressed()
    {
        bool keyboard = Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame;
        bool gamepad = Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame;
        return keyboard || gamepad;
    }

    // ── Casser (C / A-Croix) ─────────────────────────────────────────────
    public static bool BreakPressed()
    {
        bool keyboard = Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame;
        bool gamepad = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
        return keyboard || gamepad;
    }

    // ── Pause (Echap / - / Select) ───────────────────────────────────────
    public static bool PausePressed()
    {
        bool keyboard = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        // selectButton = bouton "-" (Switch) / View (Xbox) / Select (PS) — générique
        bool gamepad = Gamepad.current != null && Gamepad.current.selectButton.wasPressedThisFrame;
        return keyboard || gamepad;
    }

    // ── Navigation Quiz/Choix — Gauche/Droite uniquement ─────────────────
    /// <summary>
    /// Déplacement horizontal dans un choix/quiz.
    /// Retourne -1 (gauche), 1 (droite), 0 (rien).
    /// Clavier : Q/D + flèches. Manette : D-Pad gauche/droite + joystick droit.
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
            // D-Pad
            if (Gamepad.current.dpad.left.wasPressedThisFrame)
                return -1;
            if (Gamepad.current.dpad.right.wasPressedThisFrame)
                return 1;
            
            // Joystick droit pour navigation dans les menus/quiz
            Vector2 rightStick = Gamepad.current.rightStick.ReadValue();
            if (rightStick.x < -0.5f)
                return -1;
            if (rightStick.x > 0.5f)
                return 1;
        }

        return 0;
    }

    // ── Debug (F9) — reste clavier uniquement ────────────────────────────
    public static bool DebugTogglePressed()
    {
        return Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame;
    }

    // ── Soumettre/Valider (Espace / Y) — pour dialogues, menus ────────
    /// <summary>
    /// Utilisé pour avancer dans les dialogues et valider dans les menus UI.
    /// Clavier : Espace. Manette : Y / Triangle (buttonNorth).
    /// </summary>
    public static bool SubmitPressed()
    {
        bool keyboard = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool gamepad = Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame;
        return keyboard || gamepad;
    }

    // ── Soumettre maintenu (Espace / Y) ──────────────────────────────────
    public static bool SubmitHeld()
    {
        bool keyboard = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
        bool gamepad = Gamepad.current != null && Gamepad.current.buttonNorth.isPressed;
        return keyboard || gamepad;
    }

    // ── Confirmer UI (Espace / A) — pour valider dans l'ATH/quiz ────────
    /// <summary>
    /// Utilisé pour valider/cocher dans l'ATH (Arbre de Thoughts / quiz).
    /// Clavier : Espace. Manette : A / Croix (buttonSouth).
    /// Note : buttonSouth est aussi utilisé pour Casser (C), mais dans un contexte différent.
    /// Dans l'ATH, on ne casse pas d'arbres, donc pas de conflit pratique.
    /// </summary>
    public static bool UIConfirmPressed()
    {
        bool keyboard = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool gamepad = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
        return keyboard || gamepad;
    }

    // ── Confirmer UI maintenu (Espace / A) ────────────────────────────────
    public static bool UIConfirmHeld()
    {
        bool keyboard = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
        bool gamepad = Gamepad.current != null && Gamepad.current.buttonSouth.isPressed;
        return keyboard || gamepad;
    }
}
