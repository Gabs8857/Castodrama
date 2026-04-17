using UnityEngine;
using UnityEngine.InputSystem;

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

    private void Update()
    {
        Camera camera = GetComponent<Camera>();
        if (camera == null || !camera.orthographic)
        {
            return;
        }

        float zoomInput = 0f;
        if (Gamepad.current != null)
        {
            Vector2 dpad = Gamepad.current.dpad.ReadValue();
            zoomInput = dpad.y;
        }

        if (Mathf.Abs(zoomInput) > 0.1f)
        {
            float targetZoom = camera.orthographicSize - zoomInput * zoomSpeed * Time.deltaTime;
            targetZoom = Mathf.Max(0.01f, targetZoom);
            camera.orthographicSize = Mathf.SmoothDamp(camera.orthographicSize, targetZoom, ref zoomVelocity, smoothTime);
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }
}
