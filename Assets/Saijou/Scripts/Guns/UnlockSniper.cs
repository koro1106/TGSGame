using UnityEngine;
/// <summary>
/// スナイパー解放
/// </summary>
public class UnlockSniper : MonoBehaviour
{
    [SerializeField] PlayerStats playerStats;
    [SerializeField] GameObject sniper;
    void Start()
    {
        sniper.SetActive(playerStats.sniperUnlocked);
    }
}
