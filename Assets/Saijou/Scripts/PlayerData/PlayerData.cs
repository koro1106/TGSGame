using UnityEngine;
/// <summary>
/// プレイヤーのデータ（Exp）
/// </summary>

[CreateAssetMenu(menuName = "PlayerData")]
public class PlayerData : ScriptableObject
{
    public int currentExp; // 現在の経験値
}
