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
    // 経験値を消費してレベルアップを試みる
    public void TryLevelUp()
    {
        // レベル上限＆経験値チェック
        if (level < maxLevel && playerData.currentExp >= needExp)
        {
            // 必要な経験値を引き、レベルアップ処理を実行
            playerData.currentExp -= needExp;
            LevelUp();

            // 一回でも強化したら解放扱い
            isUnlocked = true;
            // ツールチップ更新
            TooltipUI.instance?.Show(this,false);
        }
    }

    // レベルアップ
    void LevelUp()
    {
        level++;
        isLevelUp = true;

        // レベルアップ後の必要経験値を増やす（例：1.3倍）
        needExp = Mathf.RoundToInt(needExp * 1.3f);

        // 最大レベルに到達した場合、レベルアップを停止
        if (level >= maxLevel)
        {
            level = maxLevel;
        }
    }
}