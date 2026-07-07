using UnityEngine;
using UnityEngine.Rendering.Universal;
using Ink.Runtime; // ← AJOUTE CETTE LIGNE EN HAUT !

public class Tuto_Halo_Light2D : MonoBehaviour
{
    [Header("⚡ Références")]
    [SerializeField] private Light2D playLight;

    [Header("🎨 Couleurs")]
    [SerializeField] private Color yellowColor = Color.yellow;

    [Header("📜 Variables Ink")]
    [SerializeField] private string disableLightVariableName = "disableHaloLight";

    private Color defaultLightColor;
    private bool lightEnabled = true;

    void Awake()
    {
        if (playLight != null)
        {
            defaultLightColor = playLight.color;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError($"{name}: ❌ AUCUN COLLIDER2D TROUVÉ !", this);
            return;
        }
        if (!col.isTrigger) col.isTrigger = true;
    }

    void Update()
    {
        if (playLight != null && lightEnabled)
        {
            bool shouldDisable = GetInkVariableAsBool(disableLightVariableName);
            if (shouldDisable) DisableLight();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (playLight != null && lightEnabled)
        {
            playLight.color = yellowColor;
            Debug.Log($"[Tuto_Halo_Light2D] Couleur changée en jaune", this);
        }
    }

    public void DisableLight()
    {
        if (playLight != null)
        {
            playLight.enabled = false;
            lightEnabled = false;
            Debug.Log($"[Tuto_Halo_Light2D] ⚫ Lumière désactivée", this);
        }
    }

    [ContextMenu("↩️ Réactiver la lumière")]
    public void EnableLight()
    {
        if (playLight != null)
        {
            playLight.enabled = true;
            playLight.color = defaultLightColor;
            lightEnabled = true;
            Debug.Log($"[Tuto_Halo_Light2D] ✅ Lumière réactivée", this);
        }
    }

    private bool GetInkVariableAsBool(string variableName)
    {
        if (DialogueManager.Instance?.DialogueVariables?.variables == null)
        {
            return false;
        }

        if (!DialogueManager.Instance.DialogueVariables.variables.TryGetValue(variableName, out var inkValue))
        {
            return false;
        }

        if (inkValue is BoolValue boolValue) return boolValue.value; // ← Maintenant ça marche !
        return false;
    }
}