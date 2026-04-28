using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    void Update()
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

        Vector2 gamepadInput = Vector2.zero;
        if (Gamepad.current != null)
        {
            gamepadInput = Gamepad.current.leftStick.ReadValue();
        }

        // Keep strongest source so keyboard and stick can both drive movement.
        movement = keyboardInput.sqrMagnitude >= gamepadInput.sqrMagnitude ? keyboardInput : gamepadInput;
        movement = Vector2.ClampMagnitude(movement, 1f);
    }

    void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity = movement.normalized * speed;
    }
}