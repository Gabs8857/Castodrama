using UnityEngine;

/// <summary>
/// Nuage de boue créé quand le joueur ramasse de la boue sous l'eau.
/// Flotte automatiquement en ligne droite vers le barrage (DamManager).
/// Au contact d'une fissure active, retire 1 charge (3 charges = fissure réparée).
/// Reste en jeu même si le joueur remonte à la surface.
/// </summary>
public class MudCloud : MonoBehaviour
{
    [Header("Mouvement")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Visual")]
    [SerializeField] private float bobAmplitude = 0.1f;   // amplitude du flottement vertical
    [SerializeField] private float bobFrequency = 2f;     // fréquence du flottement

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private DamManager damManager;
    private Transform damTarget;
    private bool hasReachedDam = false;
    private float spawnY;

    public static MudCloud Spawn(Vector3 position, DamManager dam)
    {
        // Crée un GameObject simple avec SpriteRenderer
        GameObject go = new GameObject("MudCloud");
        go.transform.position = position;

        // Visuel : cercle marron généré en code
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite(new Color(0.45f, 0.28f, 0.1f, 0.85f), 32);
        sr.sortingOrder = 60;
        go.transform.localScale = Vector3.one * 0.6f;

        // Collider trigger pour détecter les fissures
        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;
        col.isTrigger = true;

        // Rigidbody kinematic pour que OnTrigger fonctionne
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        MudCloud cloud = go.AddComponent<MudCloud>();
        cloud.damManager = dam;
        cloud.damTarget = dam.transform;

        return cloud;
    }

    private void Start()
    {
        spawnY = transform.position.y;

        if (damManager == null)
            damManager = FindObjectOfType<DamManager>();

        if (damTarget == null && damManager != null)
            damTarget = damManager.transform;

        if (debugLogs)
            Debug.Log($"[MudCloud] ☁ Nuage créé en {transform.position}, cible : {(damTarget != null ? damTarget.name : "NULL")}");
    }

    private void Update()
    {
        if (hasReachedDam || damTarget == null) return;

        // Mouvement en ligne droite vers le barrage (axe X uniquement)
        Vector3 dir = (damTarget.position - transform.position);
        dir.y = 0f; // ignore Y pour aller droit
        float dist = dir.magnitude;

        if (dist < 0.3f)
        {
            hasReachedDam = true;
            if (debugLogs)
                Debug.Log("[MudCloud] ☁ Nuage arrivé au barrage sans toucher de fissure — destruction");
            Destroy(gameObject, 0.5f);
            return;
        }

        // Flottement vertical sinusoïdal
        float bobY = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        Vector3 move = dir.normalized * moveSpeed * Time.deltaTime;
        move.y += bobY * Time.deltaTime;

        transform.position += move;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasReachedDam) return;

        // Vérifie si on touche un collider de fissure
        CrackColliderTag crackTag = other.GetComponent<CrackColliderTag>();
        if (crackTag != null)
        {
            if (debugLogs)
                Debug.Log($"[MudCloud] ☁ Nuage touche fissure #{crackTag.CrackIndex} !");

            bool repaired = damManager.ApplyMudCharge(crackTag.CrackIndex);
            hasReachedDam = true;

            if (debugLogs)
                Debug.Log(repaired
                    ? $"[MudCloud] ✓ Fissure #{crackTag.CrackIndex} réparée !"
                    : $"[MudCloud] Charge appliquée sur fissure #{crackTag.CrackIndex}");

            Destroy(gameObject, 0.2f);
        }
    }

    /// <summary>
    /// Génère un sprite cercle coloré en code (pas besoin de texture externe)
    /// </summary>
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
