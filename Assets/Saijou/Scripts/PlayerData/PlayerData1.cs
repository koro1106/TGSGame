using UnityEngine;
/// <summary>
/// プレイヤーのデータ（Exp）
/// </summary>

[CreateAssetMenu(menuName = "PlayerData")]
public class PlayerData : ScriptableObject
{
    public int currentExp_1; // 現在の経験値_1
    public int currentExp_2; // 現在の経験値_2
    public int currentExp_3; // 現在の経験値_3
}
