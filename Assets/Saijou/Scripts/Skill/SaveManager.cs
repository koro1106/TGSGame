using UnityEngine;
using System.IO;

/// <summary>
/// セーブ管理クラス
/// </summary>
public static class SaveManager
{
    // セーブファイル保存先
    static string path =>
        Application.persistentDataPath + "/save.json";

    /// <summary>
    /// セーブ
    /// </summary>
    public static void Save(
        PlayerData playerData,
        SkillData[] skills)
    {
        // セーブ用データ作成
        SaveData save = new SaveData();
        Debug.Log(path);
        // 経験値保存
        save.exp1 = playerData.currentExp_1;
        save.exp2 = playerData.currentExp_2;
        save.exp3 = playerData.currentExp_3;

        // スキル情報保存
        foreach (var skill in skills)
        {
            save.skills.Add(
                new SkillSaveData()
                {
                    skillName = skill.skillName,
                    level = skill.level,
                    isUnlocked = skill.isUnlocked,
                    needExp = skill.needExp
                });
        }

        // JSON化
        string json =
            JsonUtility.ToJson(save, true);
        Debug.Log("===== SAVE DATA START =====");
        Debug.Log(json);
        Debug.Log("===== SAVE DATA END =====");

        // ファイル保存
        File.WriteAllText(path, json);

        Debug.Log("セーブ完了");
    }

    /// <summary>
    /// ロード
    /// </summary>
    public static void Load(PlayerData playerData,SkillData[] skills)
    {
        Debug.Log("PATH = " + path);
        Debug.Log("EXISTS = " + File.Exists(path));
        Debug.Log("LOAD関数に入った");
        // セーブファイルが無い
        if (!File.Exists(path))
        {
            Debug.Log("セーブデータなし");
            return;
        }

        // JSON読み込み
        string json =
            File.ReadAllText(path);
        Debug.Log("===== LOAD DATA START =====");
        Debug.Log(json);
        Debug.Log("===== LOAD DATA END =====");

        // データ変換
        SaveData save =
            JsonUtility.FromJson<SaveData>(json);

        // 経験値復元
        playerData.currentExp_1 = save.exp1;
        playerData.currentExp_2 = save.exp2;
        playerData.currentExp_3 = save.exp3;


        // スキル復元
        foreach (var saveSkill in save.skills)
        {
            foreach (var skill in skills)
            {
                // 同じ名前のスキルを探す
                if (skill.skillName == saveSkill.skillName)
                {
                    skill.level =
                        saveSkill.level;

                    skill.isUnlocked =
                        saveSkill.isUnlocked;

                    skill.needExp =
                        saveSkill.needExp;
                }
            }
          
        }
    }

    /// <summary>
    /// セーブデータ削除
    /// </summary>
    public static void Delete()
    {
        // セーブファイル存在確認
        if (File.Exists(path))
        {
            File.Delete(path);

            Debug.Log("セーブ削除完了");
        }
    }
}
