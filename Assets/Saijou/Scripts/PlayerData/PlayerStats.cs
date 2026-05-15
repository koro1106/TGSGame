using UnityEngine;
/// <summary>
/// プレイヤーステータス(スキルツリーでアップするもの)
/// </summary>
[CreateAssetMenu(menuName = "PlayerStats")]
public class PlayerStats : ScriptableObject
{
    public int bulletDamage = 10; // 弾のダメージ
    public int maxAmmo = 10;      // 最大弾数

    // 解放した属性弾リスト
    public GameObject[] unlockedElementalBullets;

    // 属性弾が出る確率(一旦30％なので0.3)
    [Range(0f, 1f)]
    public float elementalBulletChance = 0.3f;
}
