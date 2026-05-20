using UnityEngine;

public class WaterSceneTransition : MonoBehaviour
{
    [SerializeField] private GameObject fondRivière;
    [SerializeField] private GameObject rivièreUpdate;
    [SerializeField] private GameObject tilemapGeneral;
    
    private Collider2D fondRivièreCollider;
    private Collider2D rivièreUpdateCollider;

    private void Start()
    {
        if (fondRivière != null)
        {
            fondRivièreCollider = fondRivière.GetComponent<Collider2D>();
        }
        
        if (rivièreUpdate != null)
        {
            rivièreUpdateCollider = rivièreUpdate.GetComponent<Collider2D>();
        }
    }

    /// <summary>
    /// Appelé quand le joueur rentre dans l'eau (TP vers la rivière)
    /// </summary>
    public void OnEnterWater()
    {
        // FondRivière devient ACTIF (affiche la rivière + trigger actif)
        if (fondRivière != null)
        {
            fondRivière.SetActive(true);
            if (fondRivièreCollider != null)
            {
                fondRivièreCollider.enabled = true;
            }
        }
        
        // Rivière update devient INACTIF (désactive son trigger pour éviter double-swim)
        if (rivièreUpdate != null)
        {
            rivièreUpdate.SetActive(false);
            if (rivièreUpdateCollider != null)
            {
                rivièreUpdateCollider.enabled = false;
            }
        }
        
        // La tilemap générale devient INACTIVE (invisibilité sous l'eau)
        if (tilemapGeneral != null)
        {
            tilemapGeneral.SetActive(false);
        }
    }

    /// <summary>
    /// Appelé quand le joueur sort de l'eau (TP vers l'extérieur)
    /// </summary>
    public void OnExitWater()
    {
        // FondRivière devient INACTIF
        if (fondRivière != null)
        {
            fondRivière.SetActive(false);
            if (fondRivièreCollider != null)
            {
                fondRivièreCollider.enabled = false;
            }
        }
        
        // Rivière update redevient ACTIF (réactive son trigger)
        if (rivièreUpdate != null)
        {
            rivièreUpdate.SetActive(true);
            if (rivièreUpdateCollider != null)
            {
                rivièreUpdateCollider.enabled = true;
            }
        }
        
        // La tilemap générale redevient ACTIVE
        if (tilemapGeneral != null)
        {
            tilemapGeneral.SetActive(true);
        }
    }

    /// <summary>
    /// Appelé lors d'un TP pour identifier la destination
    /// </summary>
    public void HandleTeleportToDestination(Transform destination)
    {
        if (destination == null)
            return;

        string destName = destination.name.ToLower();

        // Si la destination contient "eau" ou "rivière", le joueur entre dans l'eau
        if (destName.Contains("eau") || destName.Contains("rivière") || destName.Contains("water"))
        {
            OnEnterWater();
        }
        // Sinon le joueur sort de l'eau
        else
        {
            OnExitWater();
        }
    }
}
