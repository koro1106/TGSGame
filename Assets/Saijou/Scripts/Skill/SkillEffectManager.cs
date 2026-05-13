using UnityEngine;
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
            // 弾ダメージUP
            case SkillEffectType.BulletDamage:
                playerStats.bulletDamage += (int)skill.effectValue;
                break;

            // 最大弾数UP
            case SkillEffectType.MaxAmmo:
                playerStats.maxAmmo += (int)skill.effectValue;
                break;
        }
    }
}
