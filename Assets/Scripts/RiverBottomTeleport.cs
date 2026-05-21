using UnityEngine;
using UnityEngine.InputSystem;

public class RiverBottomTeleport : MonoBehaviour
{
    [SerializeField] private Transform riverBottom;
    [SerializeField] private Transform riverSurface;
    private GameObject fondRivière;
    private GameObject rivièreUpdate;
    private GameObject tilemapGeneral;
    private bool isInRiverBottomZone = false;

    private void Start()
    {
        if (riverBottom != null)
            fondRivière = riverBottom.gameObject;
        
        if (riverSurface != null)
            rivièreUpdate = riverSurface.gameObject;
        
        if (tilemapGeneral == null)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Tilemap") && obj.name.Contains("update"))
                {
                    tilemapGeneral = obj;
                    break;
                }
            }
        }

        if (rivièreUpdate != null)
            rivièreUpdate.SetActive(true);
        
        if (fondRivière != null)
            fondRivière.SetActive(false);
        
        if (tilemapGeneral != null)
        {
            foreach (SpriteRenderer renderer in tilemapGeneral.GetComponentsInChildren<SpriteRenderer>())
                renderer.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TopDownPlayerController controller = collision.GetComponent<TopDownPlayerController>();
        if (controller != null)
        {
            isInRiverBottomZone = true;
            Debug.Log("[RiverBottomTeleport] ✓ JOUEUR AU FOND!");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        TopDownPlayerController controller = collision.GetComponent<TopDownPlayerController>();
        if (controller != null)
        {
            isInRiverBottomZone = false;
        }
    }

    private void Update()
    {
        if (isInRiverBottomZone && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TeleportToRiverSurface();
        }
    }

    private void TeleportToRiverSurface()
    {
        TopDownPlayerController playerController = FindObjectOfType<TopDownPlayerController>();
        Transform player = playerController != null ? playerController.transform : null;
        
        if (player == null)
            return;

        if (riverSurface == null || riverBottom == null)
            return;

        // Déplacer le joueur
        Vector3 relativePosition = player.position - riverBottom.position;
        Vector3 newPosition = riverSurface.position + relativePosition;
        player.position = newPosition;

        // Puis gérer les transitions de visibilité
        HandleWaterSceneTransition();
        
        isInRiverBottomZone = false;
    }

    public void HandleWaterSceneTransition()
    {
        if (fondRivière != null)
            fondRivière.SetActive(false);

        if (rivièreUpdate != null)
            rivièreUpdate.SetActive(true);

        if (tilemapGeneral != null)
        {
            foreach (SpriteRenderer renderer in tilemapGeneral.GetComponentsInChildren<SpriteRenderer>())
                renderer.enabled = true;
        }
    }
}