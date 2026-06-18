using UnityEngine;
/// <summary>
/// スキル効果タイプ
/// </summary>
public enum SkillEffectType
{
    BulletDamage,          // 通常弾ダメージ
    EffectBulletDamage,    // 属性弾ダメージ
    MaxAmmo,               // 最大弾数
    UnlockElementalBullet, // 属性弾解放
    EnemySpawnWeightBonus, // 敵スポーン率ボーナス
    Criticalrate,          // クリティカル発生確率
    CriticalDamage,        // クリティカルダメージ倍率
    ExpDroprate,           // 経験値ドロップ率
    CollectionRange,       // 回収範囲増加
    PreExpGetDevice,       // プレステージExp獲得装置  
    PreExpTime,            // プレステージExp時間短縮
    CarePackage            // ケアパケ解放
}