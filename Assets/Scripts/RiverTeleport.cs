using UnityEngine;
using UnityEngine.InputSystem;

public class RiverTeleport : MonoBehaviour
{
    [SerializeField] private Transform riverSurface;
    [SerializeField] private Transform riverBottom;
    private GameObject fondRivière;
    private GameObject rivièreUpdate;
    private GameObject tilemapGeneral;
    private bool isInRiverZone = false;

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
            isInRiverZone = true;
            Debug.Log("[RiverTeleport] ✓ JOUEUR EN RIVIÈRE!");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        TopDownPlayerController controller = collision.GetComponent<TopDownPlayerController>();
        if (controller != null)
        {
            isInRiverZone = false;
        }
    }

    private void Update()
    {
        if (isInRiverZone && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TeleportToRiverBottom();
        }
    }

    private void TeleportToRiverBottom()
    {
        TopDownPlayerController playerController = FindObjectOfType<TopDownPlayerController>();
        Transform player = playerController != null ? playerController.transform : null;
        
        if (player == null)
            return;

        if (riverSurface == null || riverBottom == null)
            return;

        // Déplacer le joueur
        Vector3 relativePosition = player.position - riverSurface.position;
        Vector3 newPosition = riverBottom.position + relativePosition;
        player.position = newPosition;

        // Puis gérer les transitions de visibilité
        HandleWaterSceneTransition();
        
        isInRiverZone = false;
    }

    public void HandleWaterSceneTransition()
    {
        if (fondRivière != null)
            fondRivière.SetActive(true);

        if (rivièreUpdate != null)
            rivièreUpdate.SetActive(false);

        if (tilemapGeneral != null)
        {
            foreach (SpriteRenderer renderer in tilemapGeneral.GetComponentsInChildren<SpriteRenderer>())
                renderer.enabled = false;
        }
    }
} hello