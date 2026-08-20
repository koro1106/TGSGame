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
    public float enemySpawnWeightBonus = 0;   // 敵スポーン率ボーナス
    public int criticalrate = 0;          // クリティカル発生確率
    public int criticalDamage = 0;       // クリティカルダメージ倍率
    public int expDroprate = 0;          // 経験値ドロップ率
    public int expDroprateDouble = 0;    // ドロップ量の倍率増加
    public float collectionRange = 0.0f; // 回収範囲
    public int preExpTime = 0;           // PreExp装置時間短縮
    public int lightningBulletUP = 0;    // 雷の弾で感電する敵＋1
    public int chainBulletUP = 0;        // 鎖の弾で拘束する敵＋1
    public int poisonRangeUP = 0;        // 毒の範囲UP
    public float explosionRangeUP = 0;   // 爆発の範囲UP
    public int recoveryBulletCount = 0;  // 回復弾数UP
    public int bulletSize = 0;           // 弾の大きさUP

    // 解放した属性弾リスト
    public GameObject[] unlockedElementalBullets;

    // 属性弾が出る確率(一旦30％なので0.3)
    [Range(0f, 2f)]
    public float elementalBulletChance = 0.3f;
    public bool preExpDeviceUnlocked = false;      // PreExp装置
    public bool carePackageUnlocked = false;       // ケアパケ解放
    public bool handgunUnlocked = false;           // ハンドガン解放
    public bool handgunBulletUnlocked = false;     // ハンドガン属性弾解放
    public bool shotgunUnlocked = false;           // ショットガン解放
    public bool shotgunBulletUnlocked = false;     // ショットガン属性弾解放
    public bool sniperUnlocked = false;            // スナイパー解放
    public bool sniperBulletUnlocked = false;      // スナイパー属性弾解放

    public bool enemyAUnlocked = false;            // 解放敵＿A
    public bool enemyBUnlocked = false;            // 解放敵＿B
    public bool enemyCUnlocked = false;            // 解放敵＿C

    public bool recoveryBullet = false;            // 撃破時弾回復

    public float moveSpeed = 0f;                   // 移動速度
    public bool dash = false;                      // ダッシュ解放
    public float dashCT = 0f;                      // ダッシュCT短縮
    public bool oneShotDurability = false;         // 一発食らっても耐久解放
    public bool rapidFire = false;                 // 連射解放
    public float targetingRangeUP = 0f;            // 照準範囲拡大
    public float increasedUP = 0f;                 // 精度上昇
    public bool shopOpen = false;                  // 人形館解放
    public bool shopEffectBulletDamage = false;    // 人形館で属性ダメージ上昇解放
    public bool shopElementalBulletChance = false; // 人形館で属性属性弾確率上昇解放
    public bool shopExpChange = false;             // 人形館で素材変化解放


}
