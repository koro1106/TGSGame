using System;
using System.Collections.Generic;
/// <summary>
/// セーブデータ本体
/// </summary>
[Serializable]
public class SaveData
{
    // 経験値
    public int exp1;
    public int exp2;
    public int exp3;

    // スキル情報リスト
    public List<SkillSaveData> skills =
        new List<SkillSaveData>();
}
/// <summary>
/// スキル1つ分のセーブデータ
/// </summary>
[Serializable]
public class SkillSaveData
{
    // スキル名
    public string skillName;

    // 現在レベル
    public int level;

    // 解放済みか
    public bool isUnlocked;

    // 次レベルに必要な経験値
    public int needExp;
}