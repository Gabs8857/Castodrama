using UnityEngine;
using UnityEngine.U2D.Animation;

public class MudCloud : MonoBehaviour
{
    [Header("Mouvement")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Détection")]
    [SerializeField] private float detectionRadius = 0.8f;
    [SerializeField] private float damCollisionRadius = 1.5f;

    [Header("Collision")]
    [SerializeField] private GameObject collisionTarget;

    [Header("Animation")]
    [SerializeField] private string moveCategoryName = "MudMove";
    [SerializeField] private string[] moveFrameNames = { "Frame1", "Frame2" };
    [SerializeField] private float moveFrameSwitchSpeed = 0.15f;
    [SerializeField] private string impactCategoryName = "MudImpact";
    [SerializeField] private string[] impactFrameNames = { "Frame1", "Frame2" };
    [SerializeField] private float impactFrameSwitchSpeed = 0.1f;

    [Header("Visual")]
    [SerializeField] private Sprite mudSprite;

    [Header("Bob")]
    [SerializeField] private float bobAmplitude = 0.08f;
    [SerializeField] private float bobFrequency = 2f;

    [Header("🐛 DEBUG")]
    [SerializeField] private bool debugLogs = true;

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

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.radius = 1.2f;
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

        CircleCollider2D myCollider = GetComponent<CircleCollider2D>();
        if (myCollider == null && debugLogs)
            Debug.LogError("[MudCloud] ❌ PAS DE COLLIDER sur MudCloud !");

        if (collisionTarget != null && debugLogs)
        {
            Collider2D targetCollider = collisionTarget.GetComponent<Collider2D>();
            if (targetCollider == null)
                Debug.LogError($"[MudCloud] ❌ {collisionTarget.name} n'a PAS DE COLLIDER !");
        }

        if (spriteResolver != null)
            ApplyMoveFrame(0);
    }

    private void Update()
    {
        if (consumed)
        {
            if (Time.time - impactStartTime > impactTimeout)
                Destroy(gameObject);
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (consumed) return;

        CrackColliderTag crackTag = other.GetComponent<CrackColliderTag>();
        if (crackTag != null)
        {
            if (debugLogs) Debug.Log($"[MudCloud] 🔘 Touche FISSURE #{crackTag.CrackIndex}");
            damManager?.ApplyMudCharge(crackTag.CrackIndex);
            PlayImpactAndDestroy();
            return;
        }

        if (other.gameObject == collisionTarget || (damManager != null && other.gameObject == damManager.gameObject))
        {
            if (debugLogs) Debug.Log($"[MudCloud] 🔘 Touche BARRAGE ({other.name})");
            int nearestCrack = damManager.GetNearestActiveCrackIndex(transform.position);
            if (nearestCrack >= 0)
                damManager.ApplyMudCharge(nearestCrack);
            PlayImpactAndDestroy();
        }
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
        catch (System.Exception) { }
    }

    private void CheckCrackProximity()
    {
        if (damManager == null) return;
        int nearestCrack = damManager.GetNearestActiveCrackIndex(transform.position);
        if (nearestCrack >= 0)
        {
            Vector2 crackPos = damManager.GetCrackPosition(nearestCrack);
            if (Vector2.Distance(transform.position, crackPos) <= detectionRadius)
            {
                damManager.ApplyMudCharge(nearestCrack);
                PlayImpactAndDestroy();
                return;
            }
        }

        if (collisionTarget != null && Vector2.Distance(transform.position, collisionTarget.transform.position) <= damCollisionRadius)
        {
            int nearestCrackIndex = damManager.GetNearestActiveCrackIndex(transform.position);
            if (nearestCrackIndex >= 0)
                damManager.ApplyMudCharge(nearestCrackIndex);
            PlayImpactAndDestroy();
        }
    }

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
        catch (System.Exception) { }
    }

    // ✅ MÉTHODE CORRIGÉE (tous les chemins retournent une valeur)
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