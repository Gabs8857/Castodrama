using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class TopDownBootstrap
{
    private const string PlayerName = "Player";
    private const string GroundName = "Ground";
    private const string DangerZoneName = "ZoneDanger";
    private const string DangerBarName = "DangerBar";
    private static readonly Color PlayerBrown = new Color(0.45f, 0.25f, 0.1f, 1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
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

        SpriteRenderer spriteRenderer = playerObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = playerObject.AddComponent<SpriteRenderer>();
        }

        spriteRenderer.sprite = CreateWhiteSprite();
        spriteRenderer.color = PlayerBrown;
        spriteRenderer.sortingOrder = 10;

        TopDownHunger hunger = playerObject.GetComponent<TopDownHunger>();
        if (hunger == null)
        {
            hunger = playerObject.AddComponent<TopDownHunger>();
        }

        TopDownDanger danger = playerObject.GetComponent<TopDownDanger>();
        if (danger == null)
        {
            danger = playerObject.AddComponent<TopDownDanger>();
        }

        Rigidbody2D playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
        if (playerRigidbody == null)
        {
            playerRigidbody = playerObject.AddComponent<Rigidbody2D>();
        }

        playerRigidbody.bodyType = RigidbodyType2D.Kinematic;
        playerRigidbody.gravityScale = 0f;
        playerRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        playerRigidbody.simulated = true;

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
        {
            follow = cameraObject.AddComponent<TopDownCameraFollow>();
        }

        follow.Target = playerObject.transform;
        follow.Offset = new Vector3(0f, 0f, -10f);
        follow.SmoothTime = 0.12f;
        cameraObject.transform.position = playerObject.transform.position + new Vector3(0f, 0f, -10f);
        cameraObject.transform.rotation = Quaternion.identity;

        EnsureDangerZoneIsConfigured();
        EnsureDangerBarUiExists();

    }

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    private static void InitializeInEditor()
    {
        EditorApplication.delayCall += EnsureDangerBarInEditorHierarchy;
    }

    private static void EnsureDangerBarInEditorHierarchy()
    {
        if (Application.isPlaying)
        {
            return;
        }

        GameObject athObject = GameObject.Find("ATH");
        if (athObject == null)
        {
            return;
        }

        RectTransform athRect = athObject.GetComponent<RectTransform>();
        if (athRect == null)
        {
            return;
        }

        Transform existing = athRect.Find(DangerBarName);
        GameObject barObject = existing != null ? existing.gameObject : null;
        if (barObject == null)
        {
            barObject = new GameObject(DangerBarName);
            barObject.transform.SetParent(athRect, false);
            barObject.AddComponent<RectTransform>();
            barObject.AddComponent<TopDownDangerBarUI>();
        }
        else if (barObject.GetComponent<TopDownDangerBarUI>() == null)
        {
            barObject.AddComponent<TopDownDangerBarUI>();
        }
    }
#endif

    private static void EnsureDangerZoneIsConfigured()
    {
        GameObject zoneObject = GameObject.Find(DangerZoneName);
        if (zoneObject == null)
        {
            return;
        }

        Collider2D zoneCollider = zoneObject.GetComponent<Collider2D>();
        if (zoneCollider == null)
        {
            zoneCollider = zoneObject.AddComponent<BoxCollider2D>();
        }

        zoneCollider.isTrigger = true;

        if (zoneObject.GetComponent<DangerZoneTrigger>() == null)
        {
            zoneObject.AddComponent<DangerZoneTrigger>();
        }
    }

    private static void EnsureDangerBarUiExists()
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

        Transform parentForDangerBar = canvas.transform;
        GameObject athObject = GameObject.Find("ATH");
        if (athObject != null)
        {
            RectTransform athRect = athObject.GetComponent<RectTransform>();
            if (athRect != null)
            {
                Canvas athCanvas = athObject.GetComponentInParent<Canvas>();
                if (athCanvas != null)
                {
                    parentForDangerBar = athRect;
                }
            }
        }

        Transform existing = parentForDangerBar.Find(DangerBarName);
        GameObject barObject = existing != null ? existing.gameObject : null;
        if (barObject == null)
        {
            barObject = new GameObject(DangerBarName);
            barObject.transform.SetParent(parentForDangerBar, false);
            barObject.AddComponent<RectTransform>();
        }

        if (barObject.GetComponent<TopDownDangerBarUI>() == null)
        {
            barObject.AddComponent<TopDownDangerBarUI>();
        }
    }

    private static Sprite CreateWhiteSprite()
    {
        return Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }
}
