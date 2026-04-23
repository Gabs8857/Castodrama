using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Smoothly follows a target object with optional zoom control via gamepad D-pad.
/// Uses SmoothDamp for fluid camera movement.
/// </summary>
public class TopDownCameraFollow : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    private Vector3 offset = new Vector3(0f, 0f, -10f);

    [SerializeField]
    private float smoothTime = 0.12f;

    [SerializeField]
    private float zoomSpeed = 6f;

    [SerializeField]
    private float minZoom = 2f;

    [SerializeField]
    private float maxZoom = 15f;

    private Camera cachedCamera;
    private Vector3 velocity;
    private float zoomVelocity;

    public Transform Target
    {
        get => target;
        set => target = value;
    }

    public Vector3 Offset
    {
        get => offset;
        set => offset = value;
    }

    public float SmoothTime
    {
        get => smoothTime;
        set => smoothTime = Mathf.Max(0.01f, value);
    }

    private void Start()
    {
        cachedCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (cachedCamera == null || !cachedCamera.orthographic)
        {
            return;
        }

        UpdateZoom();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        FollowTarget();
    }

    private void UpdateZoom()
    {
        float zoomInput = GetZoomInput();

        if (Mathf.Abs(zoomInput) < 0.1f)
        {
            return;
        }

        float targetZoom = cachedCamera.orthographicSize - zoomInput * zoomSpeed * Time.deltaTime;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        
        cachedCamera.orthographicSize = Mathf.SmoothDamp(
            cachedCamera.orthographicSize,
            targetZoom,
            ref zoomVelocity,
            smoothTime
        );
    }

    private float GetZoomInput()
    {
        if (Gamepad.current == null)
        {
            return 0f;
        }

        Vector2 dpad = Gamepad.current.dpad.ReadValue();
        return dpad.y;
    }

    private void FollowTarget()
    {
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );
    }
}
