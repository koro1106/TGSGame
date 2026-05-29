using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    public EnemySpawner spawner;

    void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.BossDefeated();
        }
    }
}