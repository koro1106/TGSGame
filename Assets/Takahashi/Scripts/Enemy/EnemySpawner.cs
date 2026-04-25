using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject SquareRedInwardPrefab; // “à‘¤‚É—ˆ‚é“G
    public GameObject SquareRedWanderPrefab; // •Y‚¤“G
    public float spawnInterval = 2f;

    private float timer;

    [Range(0f, 1f)]
    public float wanderRate = 0.3f; // •Y‚¤“G‚ÌoŒ»—¦i30%j

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    void SpawnEnemy()
    {
        if (Random.value < wanderRate)
        {
            Instantiate(SquareRedWanderPrefab);
        }
        else
        {
            Instantiate(SquareRedInwardPrefab);
        }
    }
}