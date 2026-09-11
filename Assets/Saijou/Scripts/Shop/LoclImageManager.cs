using UnityEngine;

public class LoclImageManager : MonoBehaviour
{
    [Header("プレイヤーステータス")]
    [SerializeField] private PlayerStats playerStats;

    [Header("非表示にするオブジェクト")]
    [SerializeField] private GameObject[] hideObjects;

    void Update()
    {
        // どちらか一方でもtrueなら5つとも非表示
        if (playerStats.shopEffectBulletDamage || playerStats.shopElementalBulletChance)
        {
            for (int i = 0; i < hideObjects.Length; i++)
            {
                hideObjects[i].SetActive(false);
            }
        }
    }
}
