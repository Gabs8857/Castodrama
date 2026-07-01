using UnityEngine;
using UnityEngine.U2D.Animation;

public class MudCloud : MonoBehaviour
{
    [Header("Mouvement")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Détection")]
    [SerializeField] private float detectionRadius = 0.8f; // Pour les fissures
    [SerializeField] private float damCollisionRadius = 0.5f; // ✅ Rayon pour détecter le barrage

    [Header("Collision")]
    [SerializeField] private GameObject collisionTarget; // Cible de collision (barrage)

    [Header("Animation — Déplacement")]
    [SerializeField] private string moveCategoryName = "MudMove";
    [SerializeField] private string[] moveFrameNames = { "Frame1", "Frame2" };
    [SerializeField] private float moveFrameSwitchSpeed = 0.15f;

    [Header("Animation — Impact")]
    [SerializeField] private string impactCategoryName = "MudImpact";
    [SerializeField] private string[] impactFrameNames = { "Frame1", "Frame2" };
    [SerializeField] private float impactFrameSwitchSpeed = 0.1f;

    [Header("Visual fallback")]
    [SerializeField] private Sprite mudSprite;

    [Header("Bob")]
    [SerializeField] private float bobAmplitude = 0.08f;
    [SerializeField] private float bobFrequency = 2f;

    private DamManager damManager;
    private bool consumed = false;
    private bool playingImpact = false;
    private float impactTimeout = 2f;
    private float impactStartTime = 0f;

    private SpriteResolver spriteResolver;
    private SpriteRenderer spriteRenderer;
    private float timeSinceLastSwitch = 0f;
    private int currentFrameIndex = 0;

    public static MudCloud SpawnFromPrefab(GameObject prefab, Vector3 position, DamManager dam)
    {
        GameObject go = Object.Instantiate(prefab, position, Quaternion.identity);
        go.name = "MudCloud";
        MudCloud cloud = go.GetComponent<MudCloud>();
        if (cloud == null) cloud = go.AddComponent<MudCloud>();
        cloud.damManager = dam;
        cloud.collisionTarget = dam?.gameObject;
        return cloud;
    }

    public static MudCloud Spawn(Vector3 position, DamManager dam, Sprite customSprite = null)
    {
        GameObject go = new GameObject("MudCloud");
        go.transform.position = position;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = customSprite != null ? customSprite : CreateCircleSprite(new Color(0.45f, 0.28f, 0.1f, 0.85f), 32);
        sr.sortingOrder = 60;
        go.transform.localScale = Vector3.one * 0.6f;

        // ✅ Ajoute un COLLIDER TRIGGER pour détecter le barrage
        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.8f;
        col.isTrigger = true;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        MudCloud cloud = go.AddComponent<MudCloud>();
        cloud.damManager = dam;
        cloud.collisionTarget = dam?.gameObject;
        cloud.mudSprite = customSprite;
        return cloud;
    }

    private void Awake()
    {
        spriteResolver = GetComponent<SpriteResolver>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (damManager == null)
            damManager = FindObjectOfType<DamManager>();

        if (collisionTarget == null && damManager != null)
            collisionTarget = damManager.gameObject;

        if (spriteResolver != null)
            ApplyMoveFrame(0);
    }

    private void Update()
    {
        if (consumed)
        {
            if (Time.time - impactStartTime > impactTimeout)
            {
                Destroy(gameObject);
            }
            return;
        }

        if (playingImpact)
        {
            UpdateImpactAnimation();
            return;
        }

        if (damManager == null) return;

        MoveTowardsDam();
        UpdateMoveAnimation();
        CheckCrackProximity();
    }

    private void MoveTowardsDam()
    {
        if (damManager == null) return;
        Vector3 target = damManager.transform.position;
        Vector3 dir = target - transform.position;
        dir.y = 0f;

        float bobY = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        Vector3 move = dir.normalized * moveSpeed * Time.deltaTime;
        move.y += bobY * Time.deltaTime;
        transform.position += move;
    }

    private void UpdateMoveAnimation()
    {
        if (spriteResolver == null || moveFrameNames.Length == 0) return;
        timeSinceLastSwitch += Time.deltaTime;
        if (timeSinceLastSwitch >= moveFrameSwitchSpeed)
        {
            timeSinceLastSwitch = 0f;
            currentFrameIndex = (currentFrameIndex + 1) % moveFrameNames.Length;
            ApplyMoveFrame(currentFrameIndex);
        }
    }

    private void ApplyMoveFrame(int index)
    {
        try
        {
            spriteResolver.SetCategoryAndLabel(moveCategoryName, moveFrameNames[index]);
        }
        catch (System.Exception e)
        {
        }
    }

    private void CheckCrackProximity()
    {
        if (damManager == null) return;

        // ✅ 1. Détecte les fissures par proximité
        int nearestCrack = damManager.GetNearestActiveCrackIndex(transform.position);
        if (nearestCrack >= 0)
        {
            Vector2 crackPos = damManager.GetCrackPosition(nearestCrack);
            float distToCrack = Vector2.Distance(transform.position, crackPos);

            if (distToCrack <= detectionRadius)
            {
                bool repaired = damManager.ApplyMudCharge(nearestCrack);
                PlayImpactAndDestroy();
                return;
            }
        }

        // ✅ 2. Détecte le BARRAGE par proximité (si pas de fissure trouvée)
        if (collisionTarget != null)
        {
            float distToDam = Vector2.Distance(transform.position, collisionTarget.transform.position);
            if (distToDam <= damCollisionRadius)
            {
                int nearestCrackIndex = damManager.GetNearestActiveCrackIndex(transform.position);
                if (nearestCrackIndex >= 0)
                {
                    bool repaired = damManager.ApplyMudCharge(nearestCrackIndex);
                }
                PlayImpactAndDestroy();
            }
        }
    }

    /// <summary> Joue l'animation d'impact puis détruit le nuage. </summary>
    private void PlayImpactAndDestroy()
    {
        consumed = true;
        impactStartTime = Time.time;

        if (spriteResolver == null || impactFrameNames.Length == 0)
        {
            Destroy(gameObject, 0.1f);
            return;
        }

        playingImpact = true;
        currentFrameIndex = 0;
        timeSinceLastSwitch = 0f;
        ApplyImpactFrame(0);
    }

    private void UpdateImpactAnimation()
    {
        timeSinceLastSwitch += Time.deltaTime;
        if (timeSinceLastSwitch >= impactFrameSwitchSpeed)
        {
            timeSinceLastSwitch = 0f;
            currentFrameIndex++;
            if (currentFrameIndex >= impactFrameNames.Length)
            {
                Destroy(gameObject);
                return;
            }
            ApplyImpactFrame(currentFrameIndex);
        }
    }

    private void ApplyImpactFrame(int index)
    {
        try
        {
            spriteResolver.SetCategoryAndLabel(impactCategoryName, impactFrameNames[index]);
        }
        catch (System.Exception e)
        {
        }
    }

    private static Sprite CreateCircleSprite(Color color, int radius)
    {
        int size = radius * 2;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2(radius, radius);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius - 1f)
                    pixels[y * size + x] = color;
                else if (dist <= radius)
                    pixels[y * size + x] = new Color(color.r, color.g, color.b, color.a * (radius - dist));
                else
                    pixels[y * size + x] = Color.clear;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), radius);
    }
}