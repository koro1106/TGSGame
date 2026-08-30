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
    public static void Save(PlayerData playerData,SkillData[] skills)
    {
        Debug.Log("skills.Length = " + skills.Length);
        // セーブ用データ作成
        SaveData save = new SaveData();
        Debug.Log(path);
        // 経験値保存
        save.exp1 = playerData.currentExp_1;
        save.exp2 = playerData.currentExp_2;
        save.exp3 = playerData.currentExp_3;
        save.Preexp = playerData.currentPreExp;

        // スキル情報保存
        foreach(var skill in skills)
        {       
            if (skill == null)
                continue;


            SkillSaveData skillSave = new SkillSaveData();

            skillSave.skillName = skill.name;
            skillSave.level = skill.level;
            skillSave.isUnlocked = skill.isUnlocked;
            skillSave.isShopUnlocked = skill.isShopUnlocked;

            // 必要経験値を保存
            if (skill.requiredExps != null)
            {
                foreach (RequiredExp requiredExp in skill.requiredExps)
                {
                    if (requiredExp == null)
                    {
                        skillSave.requiredNeedExps.Add(0);
                    }
                    else
                    {
                        skillSave.requiredNeedExps.Add(
                            requiredExp.needExp
                        );
                    }
                }
            }

            save.skills.Add(skillSave);
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
        playerData.currentPreExp = save.Preexp;


        // スキル復元
        foreach (var saveSkill in save.skills)
        {
            foreach (var skill in skills)
            {
                if (skill == null)
                    continue;

                // 同じ名前のスキルを探す
                if (skill.name != saveSkill.skillName)
                    continue;


                // レベル復元
                skill.level =
                    saveSkill.level;

                // 解放状態復元
                skill.isUnlocked =
                    saveSkill.isUnlocked;


                // =========================
                // 必要経験値復元
                // =========================
                if (skill.requiredExps == null ||
                    saveSkill.requiredNeedExps == null)
                {
                    continue;
                }

                int count = Mathf.Min(
                    skill.requiredExps.Length,
                    saveSkill.requiredNeedExps.Count
                );

                for (int i = 0; i < count; i++)
                {
                    RequiredExp requiredExp =
                        skill.requiredExps[i];

                    if (requiredExp == null)
                        continue;

                    requiredExp.needExp =
                        saveSkill.requiredNeedExps[i];
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
