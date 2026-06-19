using UnityEngine;
/// <summary>
/// ピストル(ハンドガン)解放
/// </summary>
public class UnlockHandGun : MonoBehaviour
{
    [SerializeField] PlayerStats playerStats;
    [SerializeField] GameObject handGun;
    void Start()
    {
        handGun.SetActive(playerStats.handgunUnlocked);
    }
}
