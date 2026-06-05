using UnityEngine;
/// <summary>
/// スキル効果タイプ
/// </summary>
public enum SkillEffectType
{
    BulletDamage, // 弾ダメージ
    MaxAmmo,      // 最大弾数
    UnlockElementalBullet, // 属性弾解放
    EnemySpawnWeightBonus, // 敵スポーン率ボーナス
    Criticalrate,          // クリティカル発生確率
    CriticalDamage,        // クリティカルダメージ倍率
    ExpDroprate               // 経験値ドロップ率
}