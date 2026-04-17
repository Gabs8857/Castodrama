using UnityEngine;

public class AdaptiveHUDWidth : MonoBehaviour
{
    public enum HudAnchor
    {
        Left,
        Right
    }

    [SerializeField] private HudAnchor anchor = HudAnchor.Left;
    [SerializeField] private float targetHeightPercent = 0.24f;
    [SerializeField] private float marginXPercent = 0.03f;
    [SerializeField] private float marginYPercent = 0.03f;
    [SerializeField] private float distanceFromCamera = 10f;

    private SpriteRenderer spriteRenderer;
    private float originalHeight;
    private float originalWidth;
    private Camera mainCamera;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;

        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            // Store original sprite dimensions in world units.
            originalHeight = spriteRenderer.sprite.bounds.size.y;
            originalWidth = spriteRenderer.sprite.bounds.size.x;
        }

        AdaptToScreen();
    }

    void Update()
    {
        AdaptToScreen();
    }

    void AdaptToScreen()
    {
        if (spriteRenderer == null || mainCamera == null || originalHeight <= 0f || originalWidth <= 0f)
            return;

        float cameraHeight = mainCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        // Scale uniformly from height to keep proportions.
        float targetHeight = cameraHeight * targetHeightPercent;
        float targetScale = targetHeight / originalHeight;
        transform.localScale = new Vector3(targetScale, targetScale, 1f);

        float scaledWidth = originalWidth * targetScale;
        float scaledHeight = originalHeight * targetScale;
        float marginX = cameraWidth * marginXPercent;
        float marginY = cameraHeight * marginYPercent;

        float x = anchor == HudAnchor.Left
            ? -cameraWidth * 0.5f + scaledWidth * 0.5f + marginX
            : cameraWidth * 0.5f - scaledWidth * 0.5f - marginX;
        float y = -cameraHeight * 0.5f + scaledHeight * 0.5f + marginY;

        Vector3 cameraSpace = new Vector3(x, y, distanceFromCamera);
        transform.position = mainCamera.transform.TransformPoint(cameraSpace);
    }
}
