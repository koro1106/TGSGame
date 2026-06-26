using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// 時間経過でポイント獲得システム
/// </summary>
public class ChargePointSystem : MonoBehaviour
{
    [Header("ゲージ")]
    public Slider gaugeSlider;

    [Header("設定")]
    public float chargeGauge = 20f; // 1秒で増える量
    
    [Header("ポイントレベルシステム")]
    public PointLevelSystem pointLevelSystem;

    private float currentGauge = 0f;
    public int point = 0;


    [SerializeField] PlayerStats playerStats;
    void Update()
    {
        // 時間経過で増加
        currentGauge += (chargeGauge + playerStats.preExpTime) * Time.deltaTime;

        // UI更新
        gaugeSlider.value = currentGauge / 100f;

        // 100%以上になったら
        if(currentGauge >= 100f)
        {
            currentGauge = 0f; // リセット
            point++;           // ポイント追加

            // ポイント獲得
            point++;

            // レベルシステムへ通知
            if (pointLevelSystem != null)
            {
                pointLevelSystem.AddPoint(1);
            }
        }
    }
}
