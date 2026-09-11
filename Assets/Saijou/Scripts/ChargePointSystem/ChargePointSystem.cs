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
    public float chargeGauge = 10f; // 1秒で増える量

    private float currentGauge = 0f;

    public int point = 0;

    [SerializeField] PlayerStats playerStats;

    [Header("プレイヤーデータ")]
    [SerializeField] PlayerData playerData;


    void Start()
    {
        // PlayerDataのExp3を取得してポイントとして読み込む
        if (playerData != null)
        {
            point = playerData.currentExp_3;
        }

        // ゲージ初期化
        currentGauge = 0f;

        if (gaugeSlider != null)
        {
            gaugeSlider.value = 0f;
        }
    }


    void Update()
    {
        // 時間経過で増加
        currentGauge +=
            (chargeGauge + playerStats.preExpTime)
            * Time.deltaTime;


        // UI更新
        if (gaugeSlider != null)
        {
            gaugeSlider.value =
                currentGauge / 100f;
        }


        // 100%以上になったら
        if (currentGauge >= 100f)
        {
            currentGauge = 0f;

            // Exp3を1個取得
            AddPoint(1);
        }
    }


    /// <summary>
    /// Exp3を追加
    /// </summary>
    public void AddPoint(int amount)
    {
        // Exp3の所持数を追加
        point += amount;


        // PlayerDataにExp3として保持
        if (playerData != null)
        {
            playerData.currentExp_3 += amount;

            // pointもExp3と同期
            point = playerData.currentExp_3;
        }


        // Exp3取得ログ表示
        if (DropPickupLog.Instance != null)
        {
            DropPickupLog.Instance.ShowPickup(
                DropItemType.Exp3,
                amount
            );
        }
    }
}