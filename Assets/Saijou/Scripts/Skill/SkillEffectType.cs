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
    ExpDroprateDouble,     // ドロップ量が2倍になる確率
    CollectionRange,       // 回収範囲増加
    PreExpGetDevice,       // プレステージExp獲得装置  
    PreExpTime,            // プレステージExp時間短縮
    CarePackage,           // ケアパケ解放
    LightningBulletUP,     // 雷の弾で感電する敵＋1
    ChainBulletUP,         // 鎖の弾で拘束する敵＋1
    HandGun,               // ピストル解放
    HandGunEffectBullet,   // ピストル属性弾追加
    Shotgun,               // ショットガン解放
    ShotgunEffectBullet,   // ショットガン属性弾追加
    Sniper,                // スナイパー解放
    SniperEffectBullet,   　// スナイパー属性弾追加
}