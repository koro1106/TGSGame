using UnityEngine;
/// <summary>
/// プレイヤーステータス(スキルツリーでアップするもの)
/// </summary>
[CreateAssetMenu(menuName = "PlayerStats")]
public class PlayerStats : ScriptableObject
{
    public int bulletDamage = 10;      // 通常弾のダメージ
    public int effectBulletDamage = 0; // 属性弾のダメージ
    public int maxAmmo = 10;           // 最大弾数
    public int enemySpawnWeightBonus = 0;   // 敵スポーン率ボーナス
    public int criticalrate = 0;          // クリティカル発生確率
    public int criticalDamage = 0;        // クリティカルダメージ倍率
    public int expDroprate = 0;           // 経験値ドロップ率
    public float collectionRange = 0.0f;  // 回収範囲

    // 解放した属性弾リスト
    public GameObject[] unlockedElementalBullets;

    // 属性弾が出る確率(一旦30％なので0.3)
    [Range(0f, 1f)]
    public float elementalBulletChance = 0.3f;
    public bool preExpDeviceUnlocked = false; // PreExp装置
}
