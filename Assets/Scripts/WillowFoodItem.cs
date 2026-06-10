using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Saule (Willow) - Comestible food item with progression system.
/// First eat: frame 2 -> Second eat: frame 3 -> Third eat: disappears
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class WillowFoodItem : MonoBehaviour, IBranchSpawner
{
    [SerializeField]
    private float hungerRestoreAmount = 18f;

    [SerializeField]
    private Sprite foodSprite;

    [SerializeField]
    private Color visibleTint = new Color(0.6f, 0.8f, 0.5f, 1f);

    [SerializeField]
    private float minVisibleWorldDiameter = 0.9f;

    [SerializeField]
    private bool addGlowLight = true;

    [SerializeField]
    private float glowOuterRadius = 1.1f;

    [SerializeField]
    private float glowIntensity = 0.8f;

    [SerializeField]
    private Sprite[] eatProgressionSprites = new Sprite[2];

    [Header("Branch Spawning")]
    [SerializeField]
    private GameObject willowBranchPrefab;

    [Header("Debug")]
    [SerializeField]
    private bool debugLogs = true;

    private bool isPlayerNearby = false;
    private Collider2D currentPlayerCollider;
    private int eatCount = 0;

    private void Awake()
    {
        SetupVisuals();
        SetupPhysics();
    }

    private void SetupVisuals()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = foodSprite;
        spriteRenderer.color = visibleTint;
        spriteRenderer.sortingOrder = 200;

        if (spriteRenderer.sortingOrder < 200)
        {
            spriteRenderer.sortingOrder = 200;
        }

        if (addGlowLight)
        {
            Light2D light2D = GetComponent<Light2D>();
            if (light2D == null)
            {
                light2D = gameObject.AddComponent<Light2D>();
            }
            light2D.lightType = Light2D.LightType.Point;
            light2D.color = new Color(1f, 0.72f, 0.32f, 1f);
            light2D.intensity = Mathf.Max(0.1f, glowIntensity);
            light2D.pointLightOuterRadius = Mathf.Max(0.2f, glowOuterRadius);
            light2D.pointLightInnerRadius = Mathf.Max(0f, glowOuterRadius * 0.2f);
            light2D.falloffIntensity = 0.75f;
        }
    }

    private void SetupPhysics()
    {
        Collider2D collider = GetComponent<Collider2D>();
        collider.isTrigger = true;

        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.bodyType = RigidbodyType2D.Kinematic;
        rigidbody.gravityScale = 0f;
        rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        rigidbody.simulated = true;
    }

    private void Update()
    {
        if (isPlayerNearby && Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
        {
            TryFeedPlayer(currentPlayerCollider);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponentInParent<TopDownHunger>() != null)
        {
            isPlayerNearby = true;
            currentPlayerCollider = collision;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponentInParent<TopDownHunger>() != null)
        {
            isPlayerNearby = false;
            currentPlayerCollider = null;
        }
    }

    private void TryFeedPlayer(Collider2D collision)
    {
        TopDownHunger hungerSystem = collision.GetComponentInParent<TopDownHunger>();
        if (hungerSystem != null)
        {
            hungerSystem.AddHunger(hungerRestoreAmount);

            if (eatCount < 2)
            {
                if (eatProgressionSprites[eatCount] != null)
                {
                    SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                    spriteRenderer.sprite = eatProgressionSprites[eatCount];
                }
                eatCount++;

                // Spawne les branches quand on passe en sprite 2 (eatCount == 1)
                if (eatCount == 1)
                {
                    SpawnBranches(5, 2.5f);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Spawne des branches de saule autour de cet arbre
    /// Implémentation de IBranchSpawner
    /// </summary>
    public void SpawnBranches(int count = 3, float radius = 2f)
    {
        if (willowBranchPrefab == null)
        {
            if (debugLogs)
                Debug.LogWarning($"[WillowFoodItem] No willow branch prefab assigned for {gameObject.name}!");
            return;
        }

        if (debugLogs)
            Debug.Log($"[WillowFoodItem] ✓ Spawning {count} Willow branches around {gameObject.name}");

        for (int i = 0; i < count; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * radius;
            Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

            GameObject branchInstance = Instantiate(willowBranchPrefab, spawnPosition, Quaternion.identity);
            branchInstance.name = $"WillowBranch_{i + 1}";

            if (debugLogs)
                Debug.Log($"[WillowFoodItem] Branch #{i + 1} spawned at {spawnPosition}");
        }
    }
}
