using UnityEngine;

public class SkillTreeResetButton : MonoBehaviour
{
    [Header("リセットするスキル")]
    [SerializeField] private SkillData[] skillDatas;

    [Header("プレイヤーデータ")]
    [SerializeField] private PlayerData playerData;

    [Header("プレイヤーステータス")]
    [SerializeField] private PlayerStats playerStats;

    private void Update()
    {
        // Qキーでリセット
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ResetSkillTree();
        }
    }

    /// <summary>
    /// スキルツリーをすべて初期状態に戻す
    /// </summary>
    public void ResetSkillTree()
    {
        SaveManager.Delete();// セーブデータ削除

        // SkillDataをリセット
        foreach (SkillData skill in skillDatas)
        {
            if (skill == null)
                continue;

            skill.ResetData();
        }

        // PlayerDataをリセット
        if (playerData != null)
        {
            playerData.ResetData();
        }

        // PlayerStatsをリセット
        if (playerStats != null)
        {
            playerStats.ResetData();
        }

        Debug.Log("スキルツリーをリセットしました");
    }
}
