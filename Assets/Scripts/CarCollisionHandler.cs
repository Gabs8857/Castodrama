using UnityEngine;

/// <summary>
/// Gère les collisions entre le joueur et les voitures
/// Téléporte le joueur au spawn quand il se fait frapper par une voiture
/// </summary>
public class CarCollisionHandler : MonoBehaviour
{
    [SerializeField] private Vector3 spawnPosition = Vector3.zero; // Position de spawn du joueur (par défaut l'origine)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[CarCollisionHandler] Collision détectée avec: {collision.gameObject.name}");
        Debug.Log($"[CarCollisionHandler] Tag de l'objet: {collision.gameObject.tag}");
        Debug.Log($"[CarCollisionHandler] A un CarAnimator? {(collision.GetComponent<CarAnimator>() != null ? "OUI" : "NON")}");
        
        // Détecte si la collision est avec une voiture (tag ou composant CarAnimator)
        if (collision.CompareTag("Voiture") || collision.GetComponent<CarAnimator>() != null)
        {
            Vector3 oldPosition = transform.position;
            Debug.Log($"[CarCollisionHandler] 🚗 COLLISION AVEC UNE VOITURE !");
            Debug.Log($"[CarCollisionHandler] Position avant TP: {oldPosition}");
            
            // Téléporte le joueur au spawn
            transform.position = spawnPosition;
            
            Debug.Log($"[CarCollisionHandler] ✅ Joueur téléporté au spawn: {spawnPosition}");
            Debug.Log($"[CarCollisionHandler] La voiture: {collision.gameObject.name} a frappé le castor !");
        }
        else
        {
            Debug.Log($"[CarCollisionHandler] ❌ Collision avec un objet non-voiture, ignorée");
        }
    }
}
