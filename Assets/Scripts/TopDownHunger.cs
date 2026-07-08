using UnityEngine;
public class TopDownHunger : MonoBehaviour
{
    [SerializeField]
    private float maxHunger = 100f;

    [SerializeField]
    private float currentHunger = 100f;

    [SerializeField]
    private float drainPerSecond = 0.5f;

    [Header("Téléportation")]
    [SerializeField] private string spawnPointName = "TP_Faim";

    [Header("Récupération à la téléportation")]
    [Tooltip("Nombre d'herbes (FoodItem) mangées et faisant disparaître autour du point de spawn")]
    [SerializeField] private int grassEatenOnStarve = 2;

    [Tooltip("Rayon de recherche des herbes autour du point de spawn")]
    [SerializeField] private float grassSearchRadius = 5f;

    [Tooltip("Faim restaurée par herbe si aucun FoodItem n'est trouvé autour du spawn")]
    [SerializeField] private float fallbackHungerPerGrass = 20f;

    [Header("Fin de partie")]
    [Tooltip("Nombre de fois où la faim peut atteindre zéro avant la fin de partie")]
    [SerializeField] private int maxStarveCountBeforeEnding = 3;

    private bool hasStarved = false;
    private int starveCount = 0;
    private bool hungerDrainPaused = false;

    public float MaxHunger => maxHunger;
    public float CurrentHunger => currentHunger;
    public float NormalizedHunger => maxHunger <= 0f ? 0f : Mathf.Clamp01(currentHunger / maxHunger);
    public int StarveCount => starveCount;
    private void Update()
    {
        if (drainPerSecond <= 0f || hungerDrainPaused)
        {
            return;
        }

        currentHunger = Mathf.Max(0f, currentHunger - drainPerSecond * Time.deltaTime);

        if (currentHunger <= 0f && !hasStarved)
        {
            OnStarved();
        }

        // Réarme la détection une fois que la faim est remontée
        if (currentHunger > 0f)
            hasStarved = false;
    }

    private void OnStarved()
    {
        starveCount++;
        hasStarved = true;

        if (starveCount >= maxStarveCountBeforeEnding)
        {
            EndingManager.TriggerEnding(
                "J'aurais peut-être dû manger plus...",
                "Je recommencerai une autre fois."
            );
            return;
        }

        TeleportToSpawnAndEat();
    }

    private void TeleportToSpawnAndEat()
    {
        GameObject spawnPoint = GameObject.Find(spawnPointName);
        if (spawnPoint == null)
        {
            Debug.LogWarning($"[TopDownHunger] Point de spawn '{spawnPointName}' introuvable dans la scène !");
            return;
        }

        transform.position = spawnPoint.transform.position;

        EatGrassAround(spawnPoint.transform.position);

        Debug.Log($"[TopDownHunger] Joueur téléporté à '{spawnPointName}' et a mangé jusqu'à {grassEatenOnStarve} herbe(s). ({starveCount}/{maxStarveCountBeforeEnding})");
    }

    /// <summary>
    /// Cherche jusqu'à grassEatenOnStarve FoodItem autour du point donné et les consomme entièrement
    /// (restaure la faim et les fait disparaître). Si aucun FoodItem n'est trouvé, applique un
    /// gain de faim de secours.
    /// </summary>
    private void EatGrassAround(Vector3 position)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, grassSearchRadius);

        int eaten = 0;
        foreach (Collider2D hit in hits)
        {
            if (eaten >= grassEatenOnStarve)
                break;

            FoodItem food = hit.GetComponentInParent<FoodItem>();
            if (food != null)
            {
                food.ConsumeAndRemove(this);
                eaten++;
            }
        }

        // Si pas assez d'herbes trouvées, on compense avec un gain de secours
        int missing = grassEatenOnStarve - eaten;
        if (missing > 0)
        {
            AddHunger(fallbackHungerPerGrass * missing);
            if (eaten == 0)
                Debug.LogWarning($"[TopDownHunger] Aucune herbe trouvée autour de '{spawnPointName}' dans un rayon de {grassSearchRadius}. Gain de faim de secours appliqué.");
        }
    }

    public void SetHunger(float value)
    {
        currentHunger = Mathf.Clamp(value, 0f, maxHunger);
    }

    public void AddHunger(float amount)
    {
        SetHunger(currentHunger + amount);
    }

    public void SetHungerDrainPaused(bool paused)
    {
        hungerDrainPaused = paused;
    }

    /// <summary>
    /// Réinitialise le compteur de famine (utile pour un nouveau run après l'écran de fin).
    /// </summary>
    public void ResetStarveCount()
    {
        starveCount = 0;
        hasStarved = false;
        currentHunger = maxHunger;
    }
}
