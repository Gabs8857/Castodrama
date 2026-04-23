using UnityEngine;

public static class TopDownBootstrap
{
    private const string PlayerName = "Player";
    private const string GroundName = "Ground";
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
        cameraObject.transform.position = new Vector3(
            playerObject.transform.position.x,
            playerObject.transform.position.y + 6f,
            playerObject.transform.position.z - 8f);
        cameraObject.transform.rotation = Quaternion.Euler(35f, 0f, 0f);

    }

    private static Sprite CreateWhiteSprite()
    {
        return Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }
}
