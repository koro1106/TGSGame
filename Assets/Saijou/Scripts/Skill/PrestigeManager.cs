using UnityEngine;

public class PrestigeManager : MonoBehaviour
{
    [SerializeField] PlayerStats playerStats;
    [SerializeField] GameObject prestigeButton; // プレステージボタン

    void Update()
    {
        if (playerStats.preExpDeviceUnlocked) // プレステージ解放
        {
            prestigeButton.SetActive(true);
        }
    }
}
