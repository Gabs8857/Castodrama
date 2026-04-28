using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class TopDownPlayerController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 6.5f;

    [SerializeField]
    private float inputDeadzone = 0.12f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = Mathf.Max(0f, value);
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        moveInput = ReadMoveInput();
        if (moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }

        if (spriteRenderer != null)
        {
            if (moveInput.x < -inputDeadzone)
            {
                spriteRenderer.flipX = true;
            }
            else if (moveInput.x > inputDeadzone)
            {
                spriteRenderer.flipX = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
    }

    private Vector2 ReadMoveInput()
    {
        Vector2 keyboardInput = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                keyboardInput.x -= 1f;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                keyboardInput.x += 1f;
            }

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                keyboardInput.y -= 1f;
            }

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                keyboardInput.y += 1f;
            }
        }

        if (keyboardInput.sqrMagnitude > 1f)
        {
            keyboardInput.Normalize();
        }

        Vector2 gamepadInput = Gamepad.current != null ? Gamepad.current.leftStick.ReadValue() : Vector2.zero;
        if (gamepadInput.magnitude < inputDeadzone)
        {
            gamepadInput = Vector2.zero;
        }

        return gamepadInput.sqrMagnitude > keyboardInput.sqrMagnitude ? gamepadInput : keyboardInput;
    }
}
