using UnityEngine;

public class Danger : MonoBehaviour
{
    public float maxDanger = 100f;
    public float currentDanger = 0f;

    public float increaseSpeed = 10f;
    public float decreaseSpeed = 5f;

    private bool inForest = false;

    void Update()
    {
        if (inForest)
        {
            currentDanger += increaseSpeed * Time.deltaTime;
        }
        else
        {
            currentDanger -= decreaseSpeed * Time.deltaTime;
        }

        currentDanger = Mathf.Clamp(currentDanger, 0f, maxDanger);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Forest"))
        {
            inForest = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Forest"))
        {
            inForest = false;
        }
    }
}