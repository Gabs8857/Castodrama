using UnityEngine;
using TMPro;

/// <summary>
/// Affiche un HUD avec les touches de contrôle en haut de l'écran
/// </summary>
public class ControlsHUDUI : MonoBehaviour
{
    private TextMeshProUGUI controlsText;

    private void Awake()
    {
        // Cherche le TextMeshProUGUI dans cet objet
        controlsText = GetComponent<TextMeshProUGUI>();
        
        if (controlsText == null)
        {
            Debug.LogError("[ControlsHUDUI] Pas de TextMeshProUGUI trouvé!");
            return;
        }

        // Configure le texte
        SetupControlsText();
    }

    private void SetupControlsText()
    {
        string controlsInfo = "<b>CONTRÔLES</b>\n" +
                             "<color=yellow>F</color> - Manger les arbres\n" +
                             "<color=yellow>G</color> - Ramasser les branches\n" +
                             "<color=yellow>E</color> - Téléporter/Interagir\n" +
                             "<color=yellow>WASD</color> - Se déplacer";

        controlsText.text = controlsInfo;
        controlsText.fontSize = 36;
        controlsText.alignment = TextAlignmentOptions.Top;
    }
}
