using UnityEngine;
/// <summary>
/// PreExp取得装置解放
/// </summary>
public class PreExpDevice : MonoBehaviour
{
    [SerializeField] PlayerStats playerStats;
    [SerializeField] GameObject preExpGetDevice;

    void Start()
    {
        preExpGetDevice.SetActive(playerStats.preExpDeviceUnlocked);
    }
}
