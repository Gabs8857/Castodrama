using UnityEngine;
using System.Collections;

public class VoitureSpawner : MonoBehaviour
{
    [SerializeField] private GameObject voiturePrefab;
    [SerializeField] private Vector3 spawnPosition = new Vector3(-104f, 43f, 0f);
    [SerializeField] private Vector3 reverseSpawnPosition = new Vector3(14f, 26.5f, 0f);
    [SerializeField] private float spawnCooldown = 10f;

    private int spawnCounter = 0; // Compteur persistant pour alterner les spawns

    private void Start()
    {
        StartCoroutine(SpawnCars());
    }

    private IEnumerator SpawnCars()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnCooldown);
            
            if (spawnCounter % 2 == 0)
            {
                Instantiate(voiturePrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                Instantiate(voiturePrefab, reverseSpawnPosition, Quaternion.identity);
            }
            
            spawnCounter++;
        }
    }
}
