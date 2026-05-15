using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// スキル効果適用管理
/// </summary>
public class SkillEffectManager : MonoBehaviour
{
    [SerializeField]　PlayerStats playerStats;

   // public GunController gunController;
    /// <summary>
    /// スキル効果適用
    /// </summary>
    public void ApplySkill(SkillData skill)
    {
        switch(skill.effectType)
        {
            // 弾ダメージUP
            case SkillEffectType.BulletDamage:
                playerStats.bulletDamage += (int)skill.effectValue;
                break;

            // 最大弾数UP
            case SkillEffectType.MaxAmmo:
                playerStats.maxAmmo += (int)skill.effectValue;
                break;
            // 属性弾追加
            case SkillEffectType.UnlockElementalBullet:
                UnlockElementalBullet(skill.elementalBulletPrefab);
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

            // ここでGunControllerに追加
            //if (gunController != null)
            //{
            //    gunController.AddElementalBullet(bulletPrefab);
            //}

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
