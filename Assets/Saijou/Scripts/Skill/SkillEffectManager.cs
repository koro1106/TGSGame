using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Unity.Collections.AllocatorManager;
/// <summary>
/// スキル効果適用管理
/// </summary>
public class SkillEffectManager : MonoBehaviour
{
    [SerializeField]　PlayerStats playerStats;
    /// <summary>
    /// スキル効果適用
    /// </summary>
    public void ApplySkill(SkillData skill)
    {
        switch(skill.effectType)
        {
            // 通常弾ダメージUP
            case SkillEffectType.BulletDamage:
                playerStats.bulletDamage += (int)skill.effectValue;
                break;
            // 属性弾ダメージUP
            case SkillEffectType.EffectBulletDamage:
                playerStats.effectBulletDamage += (int)skill.effectValue;
                break;
            // 属性弾発生確率
            case SkillEffectType.ElementalBulletChance:
                playerStats.elementalBulletChance += (float)skill.effectValue;
                break;
            // 最大弾数UP
            case SkillEffectType.MaxAmmo:
                playerStats.maxAmmo += (int)skill.effectValue;
                break;
            // 属性弾追加
            case SkillEffectType.UnlockElementalBullet:
                UnlockElementalBullet(skill.elementalBulletPrefab);
                break;
            // 敵のスポーン率増加
            case SkillEffectType.EnemySpawnWeightBonus:
                playerStats.enemySpawnWeightBonus += (int)skill.effectValue;
                break;
            // クリティカル発生確率
            case SkillEffectType.Criticalrate:
                playerStats.criticalrate += (int)skill.effectValue;
                break;
            // クリティカルダメージ倍率
            case SkillEffectType.CriticalDamage:
                playerStats.criticalDamage += (int)skill.effectValue;
                break;
            // 経験値ドロップ率
            case SkillEffectType.ExpDroprate:
                playerStats.expDroprate += (int)skill.effectValue;
                break;
            // 回収範囲増加
            case SkillEffectType.CollectionRange:
                playerStats.collectionRange += (float)skill.effectValue;
                break;
            // プレステージExp獲得装置 
            case SkillEffectType.PreExpGetDevice:
                playerStats.preExpDeviceUnlocked = true;
                break;
            // プレステージExp時間短縮 
            case SkillEffectType.PreExpTime:
                playerStats.preExpTime += (int)skill.effectValue;
                break;
            // ケアパケ解放
            case SkillEffectType.CarePackage:
                playerStats.carePackageUnlocked = true;
                break;
            // 雷の弾で感電する敵＋1
            case SkillEffectType.LightningBulletUP:
                playerStats.lightningBulletUP += (int)skill.effectValue;
                break;
            // 鎖の弾で拘束する敵＋1
            case SkillEffectType.ChainBulletUP:
                playerStats.chainBulletUP += (int)skill.effectValue;
                break;
            // ピストル解放
            case SkillEffectType.HandGun:
                playerStats.handgunUnlocked = true;
                break;
            // ピストル属性弾追加
            case SkillEffectType.HandGunEffectBullet:
                playerStats.handgunBulletUnlocked = true;
                break;
            // ショットガン解放
            case SkillEffectType.Shotgun:
                playerStats.shotgunUnlocked = true;
                break;
            // ショットガン属性弾解放
            case SkillEffectType.ShotgunEffectBullet:
                playerStats.shotgunBulletUnlocked = true;
                break;
            // スナイパー解放
            case SkillEffectType.Sniper:
                playerStats.sniperUnlocked = true;
                break;
            // スナイパー解放
            case SkillEffectType.SniperEffectBullet:
                playerStats.sniperBulletUnlocked = true;
                break;
        }
    }

    /// <summary>
    /// 属性弾を解放してPlayerStatsに追加
    /// </summary>
    private void UnlockElementalBullet(GameObject bulletPrefab)
    {
        if (bulletPrefab == null) return;

        // 配列がnullなら初期化
        if (playerStats.unlockedElementalBullets == null)
            playerStats.unlockedElementalBullets = new GameObject[0];

        // すでに解放済みか確認
        if (!playerStats.unlockedElementalBullets.Contains(bulletPrefab))
        {
            var list = new List<GameObject>(playerStats.unlockedElementalBullets);
            list.Add(bulletPrefab);
            playerStats.unlockedElementalBullets = list.ToArray();

            Debug.Log("解放された属性弾: " + bulletPrefab.name);
        }
    }

    /// <summary>
    /// 指定タグの属性弾が解放されているか確認
    /// </summary>
    public bool IsElementalBulletUnlocked(string tag)
    {
        if (playerStats.unlockedElementalBullets == null) return false;
        return playerStats.unlockedElementalBullets.Any(b => b != null && b.tag == tag);
    }
}
