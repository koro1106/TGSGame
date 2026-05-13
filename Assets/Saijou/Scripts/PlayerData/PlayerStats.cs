using UnityEngine;
/// <summary>
/// プレイヤーステータス(スキルツリーでアップするもの)
/// </summary>
[CreateAssetMenu(menuName = "PlayerStats")]
public class PlayerStats : ScriptableObject
{
    public int bulletDamage = 10; // 弾のダメージ
    public int maxAmmo = 10;      // 最大弾数
}
