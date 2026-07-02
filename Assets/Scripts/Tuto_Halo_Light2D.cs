using UnityEngine;
using UnityEngine.Rendering.Universal;
/// <summary>
/// Change la couleur du halo du joueur en fonction de la variable booléenne "isHungry"
/// </summary>
public class Tuto_Halo_Light2D : MonoBehaviour
{
    [Header("⚡ Références")]
    [SerializeField] private Light2D playLight;
    [Header("🎨 Couleurs")]
    [SerializeField] private Color yellowColor = Color.yellow; // Halo jaune = faim

    private Color defaultLightColor;

    // =========================================================================
    // =========================================================================
    void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
                if (col == null)
        {
            Debug.LogError($"{name}: ❌ AUCUN COLLIDER2D TROUVÉ ! Ajoute un Box Collider 2D et coche Is Trigger.", this);
            return;
        }

        if (!col.isTrigger)
        {
            Debug.LogWarning($"{name}: ⚠️ Le Collider2D doit être en mode 'Is Trigger' !", this);
            col.isTrigger = true;
        }
    

    }
    void OnTriggerEnter2D(Collider2D other)
    {
       if (playLight != null)
            {
                playLight.color = yellowColor;
                Debug.Log($"[Tuto_Halo_Light2D] Couleur changée en jaune (faim)", this);
            }
        }
    }

    // =========================================================================
    // Récupère une variable booléenne depuis DialogueManager
    // =========================================================================

        