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

    public int currentExp = 400;   // 現在経験値
    public int needExp = 100;    // 必要経験値

    // 経験値を消費してレベルアップを試みる
    public void TryLevelUp()
    {
        // 新しく経験値を加算
        //currentExp += amount;

        // 現在のレベルが最大レベルでないことを確認
        while (level < maxLevel && currentExp >= needExp)
        {
            // 必要な経験値を引き、レベルアップ処理を実行
            currentExp -= needExp;
            LevelUp();
        }
    }

    // レベルアップ
    void LevelUp()
    {
        level++;

        // レベルアップ後の必要経験値を増やす（例：1.3倍）
        needExp = Mathf.RoundToInt(needExp * 1.3f);

        // 最大レベルに到達した場合、レベルアップを停止
        if (level >= maxLevel)
        {
            level = maxLevel;
        }
    }
}
