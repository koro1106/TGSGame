using UnityEngine;
/// <summary>
/// スキル効果タイプ
/// </summary>
public enum SkillEffectType
{
    BulletDamage,          // 通常弾ダメージ
    EffectBulletDamage,    // 属性弾ダメージ
    ElementalBulletChance, // 属性弾発生確率
    MaxAmmo,               // 最大弾数
    UnlockElementalBullet, // 属性弾解放
    EnemySpawnWeightBonus, // 敵スポーン率ボーナス
    Criticalrate,          // クリティカル発生確率
    CriticalDamage,        // クリティカルダメージ倍率
    ExpDroprate,           // 経験値ドロップ率
    ExpDroprateDouble,     // ドロップ量の倍率増加
    CollectionRange,       // 回収範囲増加
    PreExpGetDevice,       // プレステージExp獲得装置  
    PreExpTime,            // プレステージExp時間短縮
    CarePackage,           // ケアパケ解放
    LightningBulletUP,     // 雷の弾で感電する敵＋1
    ChainBulletUP,         // 鎖の弾で拘束する敵＋1
    PoisonRangeUP,         // 毒の範囲UP
    ExplosionRangeUP,      // 爆発の範囲UP
    HandGun,               // ピストル解放
    HandGunEffectBullet,   // ピストル属性弾追加
    Shotgun,               // ショットガン解放
    ShotgunEffectBullet,   // ショットガン属性弾追加
    Sniper,                // スナイパー解放
    SniperEffectBullet,  　// スナイパー属性弾追加
    EnemyUnlock_A,         // 敵解放_A
    EnemyUnlock_B,         // 敵解放_B
    EnemyUnlock_C,         // 敵解放_C
    RecoveryBullet,        // 撃破時弾回復
    RecoveryBulletCount    // 回復弾数UP
}