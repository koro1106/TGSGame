using UnityEngine;

/// <summary>
/// 必要経験値の設定
/// </summary>
[System.Serializable]
public class RequiredExp
{
    public ExpType expType;

    [Header("必要経験値初期値")]
    public int initialNeedExp;

    [Header("現在必要な経験値")]
    public int needExp;

    [Header("Lv.UPごとの増加量")]
    public int addNeedExp;

    /// <summary>
    /// 初期状態にリセット
    /// </summary>
    public void ResetData()
    {
        needExp = initialNeedExp;
    }
}
/// <summary>
/// スキルデータ（名前や説明所持）
/// </summary>
[CreateAssetMenu(menuName = "SkillData")]
public class SkillData : ScriptableObject
{
    public string skillName;  // UI表示用スキル名
    public int level;         // 現在レベル
    public int maxLevel;      // 最大レベル

    // 必要経験値を最大3種類設定
    [Header("必要経験値")]
    public RequiredExp[] requiredExps = new RequiredExp[3];

    public bool isUnlocked = false; //　解放状態
    public bool isLevelUp = false; // レベルアップした

    public PlayerData playerData;

    public SkillEffectType effectType; // スキル効果タイプ
    public float effectValue = 1;

    // 属性弾解放用
    public GameObject elementalBulletPrefab; // UnlockElementalBullet用

    [TextArea(3, 5)]
    public string description; // ショップ用
    public bool isShopUnlocked = false; // ショップスキル解放用
    public bool isShopButton = false; // ショップボタンかどうか

    [Header("ショップ人形画像")]
    public Sprite dollImage;
    public void ResetData() // リセット用
    {
        level = 0;

        foreach (RequiredExp exp in requiredExps)
        {
            if (exp != null)
            {
                exp.ResetData();
            }
        }

        isUnlocked = false;
        isLevelUp = false;
        isShopUnlocked = false;
    }
    /// <summary>
    /// 指定した経験値タイプの現在経験値を取得
    /// </summary>
    public int GetCurrentExp(ExpType expType)
    {
        switch (expType)
        {
            case ExpType.Exp1:
                return playerData.currentExp_1;

            case ExpType.Exp2:
                return playerData.currentExp_2;

            case ExpType.Exp3:
                return playerData.currentExp_3;

            case ExpType.PreExp:
                return playerData.currentPreExp;
        }

        return 0;
    }

    /// <summary>
    /// 必要経験値を満たしているか
    /// </summary>
    public bool CanLevelUp()
    {
        // 設定されている必要経験値をすべてチェック
        foreach (RequiredExp requiredExp in requiredExps)
        {
            if (requiredExp == null)
                continue;

            if (GetCurrentExp(requiredExp.expType) < requiredExp.needExp)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 経験値消費
    /// </summary>
    void ConsumeExp()
    {
        foreach (RequiredExp requiredExp in requiredExps)
        {
            if (requiredExp == null)
                continue;

            ConsumeExp(requiredExp.expType, requiredExp.needExp);
        }
    }

    /// <summary>
    /// 指定した種類の経験値を消費
    /// </summary>
    void ConsumeExp(ExpType expType, int value)
    {
        switch (expType)
        {
            case ExpType.Exp1:
                playerData.currentExp_1 -= value;
                break;

            case ExpType.Exp2:
                playerData.currentExp_2 -= value;
                break;

            case ExpType.Exp3:
                playerData.currentExp_3 -= value;
                break;

            case ExpType.PreExp:
                playerData.currentPreExp -= value;
                break;
        }
    }
    /// <summary>
    /// 経験値を消費してレベルアップ
    /// </summary>
    public void TryLevelUp()
    {
        // レベル上限＆経験値チェック
        if (level < maxLevel && CanLevelUp())
        {
            // 経験値消費
            ConsumeExp();

            // レベルアップ
            LevelUp();

            // 解放状態
            isUnlocked = true;

            if (!isShopButton)
            {
                // ツールチップ更新
                TooltipUI.instance?.ShowText(this, false);
            }
        }
    }

    /// <summary>
    /// レベルアップ
    /// </summary>
    void LevelUp()
    {
        level++;
        isLevelUp = true;

        // 必要経験値をそれぞれ増加
        foreach (RequiredExp requiredExp in requiredExps)
        {
            if (requiredExp == null)
                continue;

            requiredExp.needExp += requiredExp.addNeedExp;
        }

        // 最大レベル制限
        if (level >= maxLevel)
        {
            level = maxLevel;
        }
    }

    /// <summary>
    /// 最大レベル判定
    /// </summary>
    public bool IsMaxLevel()
    {
        return level >= maxLevel;
    }
}