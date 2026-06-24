using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq; 
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class TopDownBootstrap
{
    private const string PlayerName = "Castor";
    private const string GroundName = "Ground";
    private const string DangerZoneName = "ZoneDanger";
    private const string DangerBarName = "DangerBar";
    private const string HungerBarName = "HungerBar";
    private static readonly string[] GameSceneNames = new string[] { "Rivière", "TUTO" }; // ✅ Scènes où le bootstrap agit
    private static readonly Color PlayerBrown = new Color(0.45f, 0.25f, 0.1f, 1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded += (scene, mode) => PerformSetup();
        PerformSetup();
    }

    private static void PerformSetup()
    {
        // 🔒 Ne configure que dans les scènes autorisées
        if (!IsValidGameScene(SceneManager.GetActiveScene().name))
        {
            Debug.Log($"[TopDownBootstrap] Scène '{SceneManager.GetActiveScene().name}' ignorée");
            return;
        }

        Debug.Log($"[TopDownBootstrap] Configuration de la scène : {SceneManager.GetActiveScene().name}");

        // --- Joueur ---
        GameObject playerObject = GameObject.Find(PlayerName);
        if (playerObject == null)
        {
            playerObject = new GameObject(PlayerName);
            playerObject.transform.position = Vector3.zero;

            CircleCollider2D collider2D = playerObject.AddComponent<CircleCollider2D>();
            collider2D.radius = 0.35f;

            TopDownPlayerController controller = playerObject.AddComponent<TopDownPlayerController>();
            controller.MoveSpeed = 6.5f;
        }
        else if (playerObject.GetComponent<TopDownPlayerController>() == null)
        {
            playerObject.AddComponent<TopDownPlayerController>();
        }

        if (playerObject.GetComponent<TopDownHunger>() == null)
            playerObject.AddComponent<TopDownHunger>();

        if (playerObject.GetComponent<TopDownDanger>() == null)
            playerObject.AddComponent<TopDownDanger>();

        Rigidbody2D playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
        if (playerRigidbody == null)
            playerRigidbody = playerObject.AddComponent<Rigidbody2D>();

        playerRigidbody.bodyType = RigidbodyType2D.Dynamic;
        playerRigidbody.gravityScale = 0f;
        playerRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        playerRigidbody.simulated = true;

        // --- Sol ---
        GameObject groundObject = GameObject.Find(GroundName);
        if (groundObject == null)
        {
            groundObject = new GameObject(GroundName);
            groundObject.transform.position = Vector3.zero;

            SpriteRenderer groundRenderer = groundObject.AddComponent<SpriteRenderer>();
            groundRenderer.sprite = CreateWhiteSprite();
            groundRenderer.color = new Color(0.13f, 0.17f, 0.2f, 1f);
            groundRenderer.sortingOrder = 0;
        }

        // --- Caméra ---
        Camera camera = Camera.main;
        GameObject cameraObject = camera != null ? camera.gameObject : null;
        if (cameraObject == null)
        {
            cameraObject = new GameObject("Main Camera");
            camera = cameraObject.AddComponent<Camera>();
        }
        else
        {
            camera = cameraObject.GetComponent<Camera>();
        }

        cameraObject.tag = "MainCamera";
        camera.orthographic = true;
        camera.orthographicSize = 6f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.09f, 0.11f, 1f);

        TopDownCameraFollow follow = cameraObject.GetComponent<TopDownCameraFollow>();
        if (follow == null)
            follow = cameraObject.AddComponent<TopDownCameraFollow>();

        follow.Target = playerObject.transform;
        follow.Offset = new Vector3(0f, 0f, -10f);
        follow.SmoothTime = 0.12f;
        cameraObject.transform.position = playerObject.transform.position + new Vector3(0f, 0f, -10f);
        cameraObject.transform.rotation = Quaternion.identity;

        EnsureDangerZoneIsConfigured();
        EnsureHudUiExists();
    }

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    private static void InitializeInEditor()
    {
       EditorApplication.delayCall += EnsureHudInEditorHierarchy;
    }

    private static void EnsureHudInEditorHierarchy()
    {
        if (Application.isPlaying) return;

        GameObject athObject = GameObject.Find("ATH");
        if (athObject == null) return;

        RectTransform athRect = athObject.GetComponent<RectTransform>();
        if (athRect == null) return;

        // --- DangerBar ---
        Transform existingDanger = athRect.Find(DangerBarName);
        GameObject dangerBarObject = existingDanger != null ? existingDanger.gameObject : null;
        if (dangerBarObject == null)
        {
            dangerBarObject = new GameObject(DangerBarName);
            dangerBarObject.transform.SetParent(athRect, false);
            dangerBarObject.AddComponent<RectTransform>();
        }
        if (dangerBarObject.GetComponent<DangerBarUI>() == null)
            dangerBarObject.AddComponent<DangerBarUI>();
        HungerBarUI wrongHunger = dangerBarObject.GetComponent<HungerBarUI>();
        if (wrongHunger != null)
            Object.DestroyImmediate(wrongHunger);

        // --- HungerBar (objet séparé) ---
        Transform existingHunger = athRect.Find(HungerBarName);
        GameObject hungerBarObject = existingHunger != null ? existingHunger.gameObject : null;
        if (hungerBarObject == null)
        {
            hungerBarObject = new GameObject(HungerBarName);
            hungerBarObject.transform.SetParent(athRect, false);
            hungerBarObject.AddComponent<RectTransform>();
        }
        if (hungerBarObject.GetComponent<HungerBarUI>() == null)
            hungerBarObject.AddComponent<HungerBarUI>();
        DangerBarUI wrongDanger = hungerBarObject.GetComponent<DangerBarUI>();
        if (wrongDanger != null)
            Object.DestroyImmediate(wrongDanger);
    }
#endif

    /// <summary>
    /// Vérifie si la scène actuelle fait partie des scènes de jeu autorisées.
    /// </summary>
    private static bool IsValidGameScene(string sceneName)
    {
        return GameSceneNames.Contains(sceneName);
    }

    private static void EnsureDangerZoneIsConfigured()
    {
        GameObject zoneObject = GameObject.Find(DangerZoneName);
        if (zoneObject == null) return;

        Collider2D zoneCollider = zoneObject.GetComponent<Collider2D>();
        if (zoneCollider == null)
            zoneCollider = zoneObject.AddComponent<BoxCollider2D>();

        zoneCollider.isTrigger = true;

        if (zoneObject.GetComponent<DangerZoneTrigger>() == null)
            zoneObject.AddComponent<DangerZoneTrigger>();
    }

    private static void EnsureHudUiExists()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        Transform parentForBars = canvas.transform;
        GameObject athObject = GameObject.Find("ATH");
        if (athObject != null)
        {
            RectTransform athRect = athObject.GetComponent<RectTransform>();
            if (athRect != null && athObject.GetComponentInParent<Canvas>() != null)
                parentForBars = athRect;
        }

        // --- DangerBar ---
        Transform existingDanger = parentForBars.Find(DangerBarName);
        GameObject dangerBarObject = existingDanger != null ? existingDanger.gameObject : null;
        if (dangerBarObject == null)
        {
            dangerBarObject = new GameObject(DangerBarName);
            dangerBarObject.transform.SetParent(parentForBars, false);
            dangerBarObject.AddComponent<RectTransform>().sizeDelta = new Vector2(150f, 150f);
        }
        if (dangerBarObject.GetComponent<DangerBarUI>() == null)
            dangerBarObject.AddComponent<DangerBarUI>();
        HungerBarUI wrongHunger = dangerBarObject.GetComponent<HungerBarUI>();
        if (wrongHunger != null)
            Object.Destroy(wrongHunger);

        // --- HungerBar (objet séparé) ---
        Transform existingHunger = parentForBars.Find(HungerBarName);
        GameObject hungerBarObject = existingHunger != null ? existingHunger.gameObject : null;
        if (hungerBarObject == null)
        {
            hungerBarObject = new GameObject(HungerBarName);
            hungerBarObject.transform.SetParent(parentForBars, false);
            hungerBarObject.AddComponent<RectTransform>().sizeDelta = new Vector2(150f, 150f);
        }
        if (hungerBarObject.GetComponent<HungerBarUI>() == null)
            hungerBarObject.AddComponent<HungerBarUI>();
        DangerBarUI wrongDanger = hungerBarObject.GetComponent<DangerBarUI>();
        if (wrongDanger != null)
            Object.Destroy(wrongDanger);
    }

    private static Sprite CreateWhiteSprite()
    {
        return Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }
}