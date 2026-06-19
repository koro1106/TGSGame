using UnityEngine;
/// <summary>
/// ƒVƒ‡ƒbƒgƒKƒ“‰ð•ú
/// </summary>
public class UnlockShotGun : MonoBehaviour
{
    [SerializeField] PlayerStats playerStats;
    [SerializeField] GameObject shotGun;
    void Start()
    {
        shotGun.SetActive(playerStats.shotgunUnlocked);
    }
}
