using UnityEngine;
using UnityEngine.InputSystem;

public class RiverTeleport : MonoBehaviour
{
    [SerializeField] private GameObject fondRivièreObject;
    [SerializeField] private GameObject rivièreUpdateObject;
    [SerializeField] private GameObject tilemapGeneralObject;
    private bool isInRiverZone = false;
    private bool eKeyPressedLastFrame = false;

    private void Start()
    {
        // Auto-find if not assigned
        if (tilemapGeneralObject == null)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Tilemap") && obj.name.Contains("update"))
                {
                    tilemapGeneralObject = obj;
                    break;
                }
            }
        }

        if (rivièreUpdateObject != null)
            rivièreUpdateObject.SetActive(true);
        
        if (fondRivièreObject != null)
            fondRivièreObject.SetActive(false);
        
        if (tilemapGeneralObject != null)
        {
            foreach (SpriteRenderer renderer in tilemapGeneralObject.GetComponentsInChildren<SpriteRenderer>())
                renderer.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TopDownPlayerController controller = collision.GetComponent<TopDownPlayerController>();
        if (controller != null)
        {
            isInRiverZone = true;
            Debug.Log("[RiverTeleport] Entered zone");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        TopDownPlayerController controller = collision.GetComponent<TopDownPlayerController>();
        if (controller != null)
        {
            isInRiverZone = false;
            Debug.Log("[RiverTeleport] Exited zone");
        }
    }

    private void Update()
    {
        if (isInRiverZone && Keyboard.current != null)
        {
            bool eKeyPressed = Keyboard.current.eKey.isPressed;
            if (eKeyPressed && !eKeyPressedLastFrame)
            {
                Debug.Log("[RiverTeleport] ✓ E pressed - deep swim!");
                HandleWaterSceneTransition();
            }
            eKeyPressedLastFrame = eKeyPressed;
        }
        else
        {
            eKeyPressedLastFrame = false;
        }
    }

    public void HandleWaterSceneTransition()
    {
        Debug.Log("[RiverTeleport] HandleWaterSceneTransition called - activating deep swim mode");

        // Activer le deep swim
        CharacterAnimator animator = FindObjectOfType<CharacterAnimator>();
        if (animator != null)
        {
            animator.StartSwimmingDeep();
            Debug.Log("[RiverTeleport] ✓ Started deep swimming animation");
        }

        if (fondRivièreObject != null)
        {
            fondRivièreObject.SetActive(true);
            Debug.Log("[RiverTeleport] ✓ Activated fondRivièreObject");
        }

        if (rivièreUpdateObject != null)
        {
            rivièreUpdateObject.SetActive(false);
            Debug.Log("[RiverTeleport] ✓ Deactivated rivièreUpdateObject");
        }

        if (tilemapGeneralObject != null)
        {
            foreach (SpriteRenderer renderer in tilemapGeneralObject.GetComponentsInChildren<SpriteRenderer>())
                renderer.enabled = false;
            Debug.Log("[RiverTeleport] ✓ Disabled tilemapGeneralObject renderers");
        }
    }
}