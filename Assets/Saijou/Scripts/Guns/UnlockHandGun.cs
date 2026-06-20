using UnityEngine;
/// <summary>
/// ピストル(ハンドガン)解放
/// </summary>
public class UnlockHandGun : MonoBehaviour
{
    [SerializeField] PlayerStats playerStats;
    [SerializeField] GameObject handGun;
    [SerializeField] HandGunController handgun;
    void Start()
    {
        handGun.SetActive(playerStats.handgunUnlocked);
        if(playerStats.handgunUnlocked)
              handgun.ActivateHandGun();
    }
}
