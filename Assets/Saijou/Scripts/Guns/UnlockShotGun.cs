using UnityEngine;
/// <summary>
/// ƒVƒ‡ƒbƒgƒKƒ“‰ð•ú
/// </summary>
public class UnlockShotGun : MonoBehaviour
{
    [SerializeField] PlayerStats playerStats;
    [SerializeField] GameObject shotGun;
    [SerializeField] ShotgunController shotgun;

    void Start()
    {
        shotGun.SetActive(playerStats.shotgunUnlocked);
        if(playerStats.shotgunUnlocked)
               shotgun.ActivateShotgun();
    }
}
