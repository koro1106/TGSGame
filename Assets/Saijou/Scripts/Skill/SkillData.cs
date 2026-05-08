using UnityEngine;
/// <summary>
/// スキルデータ（名前や説明所持）
/// </summary>
[CreateAssetMenu(menuName = "SkillData")]
public class SkillData : ScriptableObject
{
    public string skillName;        // スキル名
    public string description;      // 説明文

    public int level = 1;        // 現在レベル（1?5）
    public int maxLevel = 5;    // 最大レベル

    public int needExp = 100;    // 必要経験値

    public bool isUnlocked = false; //　解放状態

    public bool isLevelUp = false; // レベルアップした
    public PlayerData playerData;
    public ExpType expType;

    /// <summary>
    /// 現在の経験値取得
    /// </summary>
    public int GetCurrentExp()
    {
        switch (expType)
        {
            case ExpType.Exp1:
                return playerData.currentExp_1;

            case ExpType.Exp2:
                return playerData.currentExp_2;

            case ExpType.Exp3:
                return playerData.currentExp_3;
        }

        return 0;
    }

    /// <summary>
    /// 経験値消費
    /// </summary>
    void ConsumeExp(int value)
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
        }
    }

    /// <summary>
    /// 経験値を消費してレベルアップ
    /// </summary>
    public void TryLevelUp()
    {
        // レベル上限＆経験値チェック
        if (level < maxLevel &&
            GetCurrentExp() >= needExp)
        {
            // 経験値消費
            ConsumeExp(needExp);

            // レベルアップ
            LevelUp();

            // 解放状態
            isUnlocked = true;

            // ツールチップ更新
            TooltipUI.instance?.Show(this, false);
        }
    }

    /// <summary>
    /// レベルアップ
    /// </summary>
    void LevelUp()
    {
        level++;
        isLevelUp = true;

        // 必要経験値増加
        needExp = Mathf.RoundToInt(needExp * 1.3f);

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