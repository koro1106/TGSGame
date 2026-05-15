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
    [Header("ポイント表示")]
    public TextMeshProUGUI pointText;
    [Header("設定")]
    public float chargeGauge = 20f; // 1秒で増える量
    [Header("プレイヤーデータ")]
    public PreStagePlayerData playerData;

    private float currentGauge = 0f;
    private int point = 0;

    public PointGetEffect pointEffectPrefab;
    public Transform pointSpawn;      // スポーン場所
    void Update()
    {
        // 時間経過で増加
        currentGauge += chargeGauge * Time.deltaTime;

        // UI更新
        gaugeSlider.value = currentGauge / 100f;

        // 100%以上になったら
        if(currentGauge >= 100f)
        {
            currentGauge = 0f; // リセット
            point++;           // ポイント追加
            pointText.text = "ポイント：" + point;

            // プレイヤーデータに反映
            if (playerData != null)
            {
                playerData.prestageExp += 1;
            }

            // Spawn位置で演出生成
            PointGetEffect effectInstance = Instantiate(
                pointEffectPrefab,
                pointSpawn.position,       // 出現場所
                Quaternion.identity,
                pointSpawn.parent          // 親は Canvas 内にしておく
            );

            // ポイント獲得演出再生
            effectInstance.PlayEffect();
        }
    }
}
