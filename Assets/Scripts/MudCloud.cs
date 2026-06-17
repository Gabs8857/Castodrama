using UnityEngine;

/// <summary>
/// Nuage de boue qui flotte vers le barrage.
/// - Touche une fissure → répare cette fissure.
/// - Touche le barrage → répare la fissure active la plus proche.
/// - Utilise UNIQUEMENT les colliders pour disparaître (plus de check de distance).
/// </summary>
public class MudCloud : MonoBehaviour
{
    [Header("Mouvement")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Visual")]
    [SerializeField] private float bobAmplitude = 0.1f;
    [SerializeField] private float bobFrequency = 2f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private DamManager damManager;
    private Transform damTarget;
    private bool hasReachedDam = false;

    public static MudCloud Spawn(Vector3 position, DamManager dam)
    {
        GameObject go = new GameObject("MudCloud");
        go.transform.position = position;

        // Visuel : cercle marron
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite(new Color(0.45f, 0.28f, 0.1f, 0.85f), 32);
        sr.sortingOrder = 60;
        go.transform.localScale = Vector3.one * 0.6f;

        // Collider TRIGGER (agrandi pour mieux détecter)
        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.8f; // Rayon effectif = 0.48 (avec scale 0.6f)
        col.isTrigger = true;

        // Rigidbody kinematic pour OnTrigger
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        MudCloud cloud = go.AddComponent<MudCloud>();
        cloud.damManager = dam;
        cloud.damTarget = dam != null ? dam.transform : null;

        return cloud;
    }

    private void Start()
    {
        if (damManager == null)
            damManager = FindObjectOfType<DamManager>();

        if (damTarget == null && damManager != null)
            damTarget = damManager.transform;

        // Destruction si pas de cible
        if (damTarget == null)
        {
            if (debugLogs)
                Debug.LogError("[MudCloud] ❌ Pas de barrage trouvé ! Destruction.");
            Destroy(gameObject);
            return;
        }

        if (debugLogs)
            Debug.Log($"[MudCloud] ☁ Créé en {transform.position}, cible: {damTarget.name}");
    }

    private void Update()
    {
        if (hasReachedDam || damTarget == null) return;

        // Mouvement horizontal vers le barrage
        Vector3 dir = (damTarget.position - transform.position);
        dir.y = 0f; // On ignore Y
        float dist = dir.magnitude;

        // Flottement vertical
        float bobY = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        Vector3 move = Vector3.zero;

        // Évite le bug NaN si dist == 0
        if (dist > 0.01f)
            move = dir.normalized * moveSpeed * Time.deltaTime;

        move.y += bobY * Time.deltaTime;
        transform.position += move;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasReachedDam) return;

        // 1️⃣ Touche une FISSURE → répare LA fissure touchée
        CrackColliderTag crackTag = other.GetComponent<CrackColliderTag>();
        if (crackTag != null)
        {
            if (debugLogs)
                Debug.Log($"[MudCloud] ☁ Touche fissure #{crackTag.CrackIndex} !");

            if (damManager != null)
            {
                bool repaired = damManager.ApplyMudCharge(crackTag.CrackIndex);
                if (debugLogs)
                    Debug.Log(repaired
                        ? $"[MudCloud] ✅ Fissure #{crackTag.CrackIndex} réparée !"
                        : $"[MudCloud] +1 charge sur fissure #{crackTag.CrackIndex}");
            }
            hasReachedDam = true;
            Destroy(gameObject, 0.2f);
            return;
        }

        // 2️⃣ Touche le BARRAGE (sans fissure) → répare la fissure ACTIVE la plus proche
        if (damManager != null && other.gameObject == damManager.gameObject)
        {
            if (debugLogs)
                Debug.Log("[MudCloud] ☁ Touche le barrage → réparation de la fissure la plus proche");

            int nearestCrackIndex = damManager.GetNearestActiveCrackIndex(transform.position);
            if (nearestCrackIndex >= 0)
            {
                bool repaired = damManager.ApplyMudCharge(nearestCrackIndex);
                if (debugLogs)
                    Debug.Log(repaired
                        ? $"[MudCloud] ✅ Fissure #{nearestCrackIndex} réparée par proximité !"
                        : $"[MudCloud] +1 charge sur fissure #{nearestCrackIndex}");
            }
            else if (debugLogs)
            {
                Debug.Log("[MudCloud] ⚠️ Aucune fissure active à réparer");
            }
            hasReachedDam = true;
            Destroy(gameObject, 0.2f);
        }
    }

    // Génération du sprite (inchangé)
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