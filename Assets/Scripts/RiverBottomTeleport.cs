using UnityEngine;
using UnityEngine.InputSystem;

public class RiverBottomTeleport : MonoBehaviour
{
    [SerializeField] private GameObject fondRivièreObject;
    [SerializeField] private GameObject rivièreUpdateObject;
    [SerializeField] private GameObject tilemapGeneralObject;
    private bool isInRiverBottomZone = false;
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
            isInRiverBottomZone = true;
            Debug.Log("[RiverBottomTeleport] Entered zone");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        TopDownPlayerController controller = collision.GetComponent<TopDownPlayerController>();
        if (controller != null)
        {
            isInRiverBottomZone = false;
            Debug.Log("[RiverBottomTeleport] Exited zone");
        }
    }

    private void Update()
    {
        if (isInRiverBottomZone && Keyboard.current != null)
        {
            bool eKeyPressed = Keyboard.current.eKey.isPressed;
            if (eKeyPressed && !eKeyPressedLastFrame)
            {
                Debug.Log("[RiverBottomTeleport] ✓ E pressed - rising to surface!");
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
        Debug.Log("[RiverBottomTeleport] HandleWaterSceneTransition called - returning to surface");

        // Désactiver le deep swim
        CharacterAnimator animator = FindObjectOfType<CharacterAnimator>();
        if (animator != null)
        {
            animator.StopSwimmingDeep();
            Debug.Log("[RiverBottomTeleport] ✓ Stopped deep swimming animation");
        }

        if (fondRivièreObject != null)
        {
            fondRivièreObject.SetActive(false);
            Debug.Log("[RiverBottomTeleport] ✓ Deactivated fondRivièreObject");
        }

        if (rivièreUpdateObject != null)
        {
            rivièreUpdateObject.SetActive(true);
            Debug.Log("[RiverBottomTeleport] ✓ Activated rivièreUpdateObject");
        }

        if (tilemapGeneralObject != null)
        {
            foreach (SpriteRenderer renderer in tilemapGeneralObject.GetComponentsInChildren<SpriteRenderer>())
                renderer.enabled = true;
            Debug.Log("[RiverBottomTeleport] ✓ Enabled tilemapGeneralObject renderers");
        }
    }
}