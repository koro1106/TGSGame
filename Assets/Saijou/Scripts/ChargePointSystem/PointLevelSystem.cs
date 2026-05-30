using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// ポイント管理システム
/// ・ポイントを受け取る
/// ・100ポイント貯まるとレベルポイント獲得
/// ・UI更新
/// </summary>
public class PointLevelSystem : MonoBehaviour
{
    [Header("経験値ゲージ")]
    public Slider expSlider;

    [Header("経験値表示")]
    public TextMeshProUGUI expText;

    [Header("レベルポイント表示")]
    public TextMeshProUGUI levelText;

    [Header("プレイヤーデータ")]
    public PreStagePlayerData playerData;

    // 現在の経験値（0～99）
    private int currentExp = 0;

    // レベルポイント
    private int levelPoint = 0;


    // ポイント追加
    public void AddPoint(int amount)
    {
        // ポイント加算
        currentExp += amount;

        // 100以上になったらレベルポイント獲得
        while (currentExp >= 10)
        {
            // 100消費
            currentExp -= 10;

            // レベルポイント獲得
            levelPoint++;

            // プレイヤーデータに反映
            if (playerData != null)
            {
                playerData.prestageExp += 1;
            }

            Debug.Log("レベルポイント獲得！ 現在：" + levelPoint);
        }

        // UI更新
        UpdateUI();
    }

    // UI更新処理
    private void UpdateUI()
    {
        // スライダー更新
        expSlider.value = currentExp;

        // 経験値表示
        expText.text = currentExp + " / 10";

        // レベルポイント表示
        levelText.text = "レベルポイント：" + levelPoint;
    }
}
