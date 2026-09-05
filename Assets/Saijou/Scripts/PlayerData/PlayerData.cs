using UnityEngine;
/// <summary>
/// プレイヤーのデータ（Exp）
/// </summary>

[CreateAssetMenu(menuName = "PlayerData")]
public class PlayerData : ScriptableObject
{
    // 経験値最大値
    public const int MaxExp = 9999;

    public int currentExp_1; // 現在の経験値_1
    public int currentExp_2; // 現在の経験値_2
    public int currentExp_3; // 現在の経験値_3
    public int currentPreExp; // 現在のプレステージ用経験値

    /// <summary>
    /// 経験値をすべて初期化
    /// </summary>
    public void ResetData()
    {
        currentExp_1 = 0;
        currentExp_2 = 0;
        currentExp_3 = 0;
        currentPreExp = 0;
    }
}

