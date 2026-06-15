using UnityEngine;

/// <summary>
/// Gère la repousse des herbes (FoodItem) à chaque nouveau jour.
/// Au lieu de détruire/réinstancier, réinitialise les herbes existantes.
/// Compatible avec les herbes qui ne sont pas des prefabs.
/// </summary>
public class GrassSpawner : MonoBehaviour
{
    [Tooltip("Le GameObject parent qui contient tous les groupes d'herbes (ex: 'Grass')")]
    [SerializeField] private Transform grassParent;

    private FoodItem[] allGrass;

    private void Awake()
    {
        GameState.grassSpawner = this;

        if (grassParent == null)
        {
            Debug.LogError("[GrassSpawner] ✗ Aucun parent d'herbes assigné dans l'Inspector !");
            return;
        }

        allGrass = grassParent.GetComponentsInChildren<FoodItem>(true); // true = inclut les inactifs
        Debug.Log($"[GrassSpawner] ✓ {allGrass.Length} herbe(s) détectée(s) sous '{grassParent.name}'.");
    }

    /// <summary>
    /// Réinitialise toutes les herbes (réactive celles mangées, remet leur sprite d'origine).
    /// Appelé par DayManager au début de chaque nouveau jour.
    /// </summary>
    public void RespawnAll()
    {
        if (allGrass == null || allGrass.Length == 0)
        {
            Debug.LogWarning("[GrassSpawner] Aucune herbe enregistrée, impossible de respawner.");
            return;
        }

        int respawned = 0;
        foreach (FoodItem grass in allGrass)
        {
            if (grass == null) continue;
            grass.gameObject.SetActive(true);
            grass.ResetState();
            respawned++;
        }

        Debug.Log($"[GrassSpawner] ✓ {respawned} herbe(s) repoussée(s) pour le nouveau jour.");
    }
}
