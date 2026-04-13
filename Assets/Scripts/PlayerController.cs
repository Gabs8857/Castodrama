using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float playerLightIntensity = 1.35f;
    public float playerLightOuterRadius = 6f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Light2D playerLight;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerLight = GetComponent<Light2D>();
        ConfigureLighting();
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
        rb.linearVelocity = movement.normalized * speed;
    }

    void ConfigureLighting()
    {
        if (playerLight != null)
        {
            playerLight.enabled = true;
            playerLight.lightType = Light2D.LightType.Point;
            playerLight.intensity = playerLightIntensity;
            playerLight.pointLightOuterRadius = playerLightOuterRadius;
            playerLight.pointLightInnerRadius = 1.2f;
            playerLight.color = new Color(1f, 0.9f, 0.63f, 1f);
        }

        GameObject globalLightObject = GameObject.Find("Global Light 2D");
        if (globalLightObject == null)
        {
            return;
        }

        Light2D globalLight = globalLightObject.GetComponent<Light2D>();
        if (globalLight != null && globalLight.intensity > 0.2f)
        {
            globalLight.intensity = 0.15f;
        }
    }
}