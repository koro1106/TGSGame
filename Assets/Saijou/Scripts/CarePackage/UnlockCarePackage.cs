using UnityEngine;
/// <summary>
/// ケアパケ装置解放
/// </summary>
public class UnlockCarePackage : MonoBehaviour
{
    [SerializeField] PlayerStats playerStats;
    [SerializeField] GameObject carePackage;
    void Start()
    {
        carePackage.SetActive(playerStats.carePackageUnlocked);
    }
}
