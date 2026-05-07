using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Classe de base pour les contrôleurs de joueur
/// Gère le mouvement basic (clavier + gamepad)
/// 
/// À étendre pour ajouter des fonctionnalités spéciales (inventaire, zones, etc.)
/// Voir: TopDownPlayerController pour un exemple complet
/// </summary>
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    protected float moveSpeed = 5f;

    [SerializeField]
    protected float inputDeadzone = 0.12f;

    protected Rigidbody2D rb;
    protected Vector2 moveInput;
    
    /// <summary>
    /// Propriété publique pour accéder/modifier la vitesse de mouvement
    /// </summary>
    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    protected virtual void Update()
    {
        moveInput = ReadMoveInput();
        if (moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity = moveInput * moveSpeed;
    }

    /// <summary>
    /// Lit l'entrée du joueur (clavier + gamepad)
    /// À surcharger pour ajouter d'autres entrées
    /// </summary>
    protected virtual Vector2 ReadMoveInput()
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

        if (gamepadInput.magnitude < inputDeadzone)
        {
            gamepadInput = Vector2.zero;
        }

        return gamepadInput.sqrMagnitude > keyboardInput.sqrMagnitude ? gamepadInput : keyboardInput;
    }}