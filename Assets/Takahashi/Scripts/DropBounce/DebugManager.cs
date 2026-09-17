using UnityEngine;

/// <summary>
/// デバッグ用の一括加算スクリプト
/// シーンに1つだけ配置する（GameManager的な立ち位置）
/// </summary>
public class DebugManager : MonoBehaviour
{
    [Header("参照")]
    public PlayerData playerData;
    public SkillData[] allSkills;

    [Header("デバッグ：一括加算")]
    [Tooltip("このキーを押すと全経験値に加算")]
    public KeyCode debugAddKey = KeyCode.F9;

    public int debugAddAmount = 100;

    void Update()
    {
        // =====================================================
        // キーで全経験値+指定量
        // =====================================================
        if (Input.GetKeyDown(debugAddKey))
        {
            DebugAddAllExp();
        }
    }

    // =========================================================
    // 全経験値へまとめて加算
    // =========================================================
    void DebugAddAllExp()
    {
        if (playerData == null)
        {
            Debug.LogWarning("DebugManager: playerDataが未設定です");
            return;
        }

        playerData.currentExp_1 =
            Mathf.Min(playerData.currentExp_1 + debugAddAmount, PlayerData.MaxExp);

        playerData.currentExp_2 =
            Mathf.Min(playerData.currentExp_2 + debugAddAmount, PlayerData.MaxExp);

        playerData.currentExp_3 =
            Mathf.Min(playerData.currentExp_3 + debugAddAmount, PlayerData.MaxExp);

        playerData.currentPreExp =
            Mathf.Min(playerData.currentPreExp + debugAddAmount, PlayerData.MaxExp);

        SaveManager.Save(playerData, allSkills);

        Debug.Log("【デバッグ】全経験値+" + debugAddAmount);
    }
}