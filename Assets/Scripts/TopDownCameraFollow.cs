using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Smoothly follows a target object with optional zoom control via gamepad D-pad.
/// Uses SmoothDamp for fluid camera movement.
/// </summary>
public class TopDownCameraFollow : MonoBehaviour
{
    private enum CameraViewMode
    {
        TopDown,
        PokemonThreeQuarter
    }

    [SerializeField]
    private Transform target;

    [SerializeField]
    private CameraViewMode viewMode = CameraViewMode.PokemonThreeQuarter;

    [SerializeField]
    private Vector3 offset = new Vector3(0f, 0f, -10f);

    [SerializeField]
    private float followHeight = 6f;

    [SerializeField]
    private float followDistance = 8f;

    [SerializeField]
    private float pitchAngle = 35f;

    [SerializeField]
    private float yawAngle = 0f;

    [SerializeField]
    private float rotationSmoothTime = 0.1f;

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
    private Vector3 rotationVelocity;
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

        if (viewMode == CameraViewMode.PokemonThreeQuarter)
        {
            FollowTargetThreeQuarter();
            return;
        }

        FollowTargetTopDown();
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

    private void FollowTargetTopDown()
    {
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );

        transform.rotation = Quaternion.identity;
    }

    private void FollowTargetThreeQuarter()
    {
        Quaternion desiredRotation = Quaternion.Euler(pitchAngle, yawAngle, 0f);
        Vector3 backward = desiredRotation * Vector3.back;
        Vector3 desiredPosition = target.position + Vector3.up * Mathf.Max(0f, followHeight) + backward * Mathf.Max(0f, followDistance);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );

        Vector3 currentEuler = transform.rotation.eulerAngles;
        Vector3 targetEuler = desiredRotation.eulerAngles;
        Vector3 nextEuler = Vector3.SmoothDamp(currentEuler, targetEuler, ref rotationVelocity, Mathf.Max(0.01f, rotationSmoothTime));
        transform.rotation = Quaternion.Euler(nextEuler);
    }
}
