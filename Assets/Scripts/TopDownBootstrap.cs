using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public static class TopDownBootstrap
{
    private const string PlayerName = "Player";
    private const string GroundName = "Ground";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        GameObject playerObject = GameObject.Find(PlayerName);
        if (playerObject == null)
        {
            playerObject = new GameObject(PlayerName);
            playerObject.transform.position = Vector3.zero;

            SpriteRenderer spriteRenderer = playerObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateWhiteSprite();
            spriteRenderer.color = new Color(0.23f, 0.68f, 1f, 1f);
            spriteRenderer.sortingOrder = 10;

            CircleCollider2D collider2D = playerObject.AddComponent<CircleCollider2D>();
            collider2D.radius = 0.35f;

            TopDownPlayerController controller = playerObject.AddComponent<TopDownPlayerController>();
            controller.MoveSpeed = 6.5f;
        }
        else if (playerObject.GetComponent<TopDownPlayerController>() == null)
        {
            playerObject.AddComponent<TopDownPlayerController>();
        }

        TopDownHunger hunger = playerObject.GetComponent<TopDownHunger>();
        if (hunger == null)
        {
            hunger = playerObject.AddComponent<TopDownHunger>();
        }

        GameObject globalLightObject = GameObject.Find("Global Light 2D");
        if (globalLightObject != null)
        {
            Light2D globalLight = globalLightObject.GetComponent<Light2D>();
            if (globalLight != null)
            {
                globalLight.enabled = false;
            }
        }

        GameObject playerLightObject = GameObject.Find("Player Point Light");
        if (playerLightObject == null)
        {
            playerLightObject = new GameObject("Player Point Light");
        }

        playerLightObject.transform.SetParent(playerObject.transform, false);
        playerLightObject.transform.localPosition = new Vector3(0f, 0.5f, 0f);

        Light2D pointLight = playerLightObject.GetComponent<Light2D>();
        if (pointLight == null)
        {
            pointLight = playerLightObject.AddComponent<Light2D>();
        }

        pointLight.lightType = Light2D.LightType.Point;
        pointLight.pointLightInnerRadius = 3.5f;
        pointLight.pointLightOuterRadius = 10f;
        pointLight.falloffIntensity = 0.82f;
        pointLight.intensity = 1.35f;
        pointLight.color = new Color(1f, 0.86f, 0.58f, 1f);
        pointLight.lightOrder = 0;
        pointLight.blendStyleIndex = 0;

        GameObject groundObject = GameObject.Find(GroundName);
        if (groundObject == null)
        {
            groundObject = new GameObject(GroundName);
            groundObject.transform.position = Vector3.zero;

            SpriteRenderer groundRenderer = groundObject.AddComponent<SpriteRenderer>();
            groundRenderer.sprite = CreateWhiteSprite();
            groundRenderer.color = new Color(0.13f, 0.17f, 0.2f, 1f);
            groundRenderer.sortingOrder = 0;
            groundObject.transform.localScale = new Vector3(60f, 34f, 1f);
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
        cameraObject.transform.position = new Vector3(playerObject.transform.position.x, playerObject.transform.position.y, -10f);
        cameraObject.transform.rotation = Quaternion.identity;

        GameObject hudCanvasObject = GameObject.Find("HUD Canvas");
        if (hudCanvasObject == null)
        {
            hudCanvasObject = new GameObject("HUD Canvas");
            Canvas canvas = hudCanvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler canvasScaler = hudCanvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.matchWidthOrHeight = 0.5f;

            hudCanvasObject.AddComponent<GraphicRaycaster>();
        }

        GameObject hungerFrame = GameObject.Find("Hunger Frame");
        if (hungerFrame == null)
        {
            hungerFrame = new GameObject("Hunger Frame");
            hungerFrame.transform.SetParent(hudCanvasObject.transform, false);

            RectTransform frameRect = hungerFrame.AddComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0f, 0f);
            frameRect.anchorMax = new Vector2(0f, 0f);
            frameRect.pivot = new Vector2(0f, 0f);
            frameRect.anchoredPosition = new Vector2(24f, 24f);
            frameRect.sizeDelta = new Vector2(240f, 20f);

            Image backgroundImage = hungerFrame.AddComponent<Image>();
            backgroundImage.sprite = CreateWhiteSprite();
            backgroundImage.color = new Color(0f, 0f, 0f, 0.55f);

            GameObject hungerFill = new GameObject("Hunger Fill");
            hungerFill.transform.SetParent(hungerFrame.transform, false);

            RectTransform fillRect = hungerFill.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = new Vector2(3f, 3f);
            fillRect.offsetMax = new Vector2(-3f, -3f);

            Image fillImage = hungerFill.AddComponent<Image>();
            fillImage.sprite = CreateWhiteSprite();
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 1f;

            TopDownHungerBarUI hungerBarUI = hungerFrame.AddComponent<TopDownHungerBarUI>();
            hungerBarUI.Hunger = hunger;
            hungerBarUI.FillImage = fillImage;
        }
        else
        {
            TopDownHungerBarUI hungerBarUI = hungerFrame.GetComponent<TopDownHungerBarUI>();
            if (hungerBarUI == null)
            {
                hungerBarUI = hungerFrame.AddComponent<TopDownHungerBarUI>();
            }

            hungerBarUI.Hunger = hunger;
            Image fillImage = hungerFrame.transform.Find("Hunger Fill")?.GetComponent<Image>();
            if (fillImage != null)
            {
                hungerBarUI.FillImage = fillImage;
            }
        }
    }

    private static Sprite CreateWhiteSprite()
    {
        return Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }
}
