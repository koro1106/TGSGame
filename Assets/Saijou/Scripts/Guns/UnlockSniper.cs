using UnityEngine;
/// <summary>
/// スナイパー解放
/// </summary>
public class UnlockSniper : MonoBehaviour
{
    [SerializeField] PlayerStats playerStats;
    [SerializeField] GameObject sniper;
    [SerializeField] SniperController Sniper;

    void Start()
    {
        sniper.SetActive(playerStats.sniperUnlocked);
        if(playerStats.sniperUnlocked)
                 Sniper.ActivateSniper();

    }
}
