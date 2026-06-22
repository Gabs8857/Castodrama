using UnityEngine;

/// <summary>
/// Point d'intérêt :
/// - Disparaît quand le joueur le traverse.
/// - Met une variable INK à true (pour ton collègue).
/// - Une fois traversé, disparaît pour toujours (même après reload).
/// </summary>
public class PointOfInterest : MonoBehaviour
{
    [Header("📜 INK Integration")]
    [Tooltip("Nom de la variable INK à activer (ex: 'poi_forest_visited')")]
    [SerializeField] private string inkVariableName = "poi_visited";

    [Header("⚙️ Settings")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private bool persistAfterReload = true; // Garder la disparition après reload ?

    private void Start()
    {
        // Si le point a déjà été visité, on le détruit immédiatement
        if (persistAfterReload && PlayerPrefs.GetInt(inkVariableName, 0) == 1)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Vérifie si c'est le joueur
        if (!IsPlayer(other)) return;

        // ✅ Met la variable INK (à adapter selon ton système)
        SetInkVariable(inkVariableName, true);

        // ✅ Sauvegarde l'état (si activé)
        if (persistAfterReload)
            PlayerPrefs.SetInt(inkVariableName, 1);

        if (debugLogs)
            Debug.Log($"[PointOfInterest] {gameObject.name} traversé ! Variable INK '{inkVariableName}' = true");

        // ✅ Détruit le point d'intérêt (définitif)
        Destroy(gameObject);
    }

    /// <summary>
    /// Vérifie si le collider appartient au joueur.
    /// </summary>
    private bool IsPlayer(Collider2D collider)
    {
        return collider.CompareTag("Player") || collider.GetComponent<TopDownPlayerController>() != null;
    }

    /// <summary>
    /// Méthode à adapter selon ton système INK.
    /// Par défaut : utilise PlayerPrefs (simple et compatible partout).
    /// </summary>
    private void SetInkVariable(string variableName, bool value)
    {
        // 🔹 OPTION 1 : Si tu utilises un singleton INK (ex: InkStoryManager)
        // if (InkStoryManager.Instance != null)
        //     InkStoryManager.Instance.SetVariable(variableName, value);

        // 🔹 OPTION 2 : PlayerPrefs (fonctionne partout, même sans INK)
        PlayerPrefs.SetInt(variableName, value ? 1 : 0);
        PlayerPrefs.Save();

        // 🔹 OPTION 3 : Événement custom (si ton collègue écoute des événements)
        // EventBus.Publish(new InkVariableChangedEvent(variableName, value));
    }
}