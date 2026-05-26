using UnityEngine;
using System.Collections;

public class VoitureSpawner : MonoBehaviour
{
    [SerializeField] private GameObject voiturePrefab;
    [SerializeField] private Vector3 spawnPosition = new Vector3(-104f, 43f, 0f);
    [SerializeField] private float spawnCooldown = 10f;

    private void Start()
    {
        StartCoroutine(SpawnCars());
    }

    private IEnumerator SpawnCars()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnCooldown);
            Instantiate(voiturePrefab, spawnPosition, Quaternion.identity);
        }
    }
}
