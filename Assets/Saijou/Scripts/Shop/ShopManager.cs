using UnityEngine;

public class ShopManager : MonoBehaviour
{

    [SerializeField] PlayerStats playerStats;
    [SerializeField] GameObject shopButton; // ショップボタン

    void Update()
    {
        if(playerStats.shopOpen) // 人形館解放
        {
            shopButton.SetActive(true);
        }
    }
}
