using UnityEngine;
using UnityEngine.InputSystem;

public class InvisibleWallsManager : MonoBehaviour
{
    [SerializeField] private GameObject invisibleWallsObject;
    private bool isInRiverZone = false;
    private bool eKeyPressedLastFrame = false;
    private bool wallsAreVisible = false; // false = surface, true = deep

    private void Start()
    {
        // Auto-find invisible walls tilemap if not assigned
        if (invisibleWallsObject == null)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("RivRivièreFond") && obj.name.Contains("update"))
                {
                    invisibleWallsObject = obj;
                    Debug.Log("[InvisibleWallsManager] ✓ Found invisible walls: " + obj.name);
                    break;
                }
            }
        }

        if (invisibleWallsObject == null)
            Debug.Log("[InvisibleWallsManager] ⚠ Could not find invisible walls!");

        // Murs invisibles cachés au démarrage (surface)
        if (invisibleWallsObject != null)
        {
            foreach (SpriteRenderer renderer in invisibleWallsObject.GetComponentsInChildren<SpriteRenderer>())
                renderer.enabled = false;
            wallsAreVisible = false;
            Debug.Log("[InvisibleWallsManager] ✓ Walls hidden at start");
        }
    }

    private void Update()
    {
        if (isInRiverZone && Keyboard.current != null)
        {
            bool eKeyPressed = Keyboard.current.eKey.isPressed;
            if (eKeyPressed && !eKeyPressedLastFrame)
            {
                ToggleInvisibleWalls();
            }
            eKeyPressedLastFrame = eKeyPressed;
        }
        else
        {
            eKeyPressedLastFrame = false;
        }
    }

    private void ToggleInvisibleWalls()
    {
        if (invisibleWallsObject == null)
        {
            Debug.Log("[InvisibleWallsManager] ⚠ Cannot toggle - walls object is NULL");
            return;
        }

        wallsAreVisible = !wallsAreVisible;

        foreach (SpriteRenderer renderer in invisibleWallsObject.GetComponentsInChildren<SpriteRenderer>())
            renderer.enabled = wallsAreVisible;

        string state = wallsAreVisible ? "VISIBLE (deep)" : "HIDDEN (surface)";
        Debug.Log($"[InvisibleWallsManager] ✓ Walls toggled to: {state}");
    }

    // Appeler depuis RiverTeleport quand E est pressé à la surface
    public void OnEnterRiverZone()
    {
        isInRiverZone = true;
        Debug.Log("[InvisibleWallsManager] ✓ Entered river zone");
    }

    // Appeler depuis RiverBottomTeleport quand on quitte la zone profonde
    public void OnExitRiverZone()
    {
        isInRiverZone = false;
        Debug.Log("[InvisibleWallsManager] ✓ Exited river zone");
    }
}
