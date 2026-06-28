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

    [Header("プレイヤーデータ")]
    public PlayerData playerData;

    [Header("経験値プレハブ")]
    public GameObject expPrefab;

    [Header("プレイヤー")]
    public Transform player;

    public int spawnCount = 1;   // 何個出すか

    // 現在の経験値（0～99）
    private int currentExp = 0;

    // レベルポイント
    private int levelPoint = 0;

    public PointGetEffect pointEffectPrefab;
    //public Transform pointSpawn;      // スポーン場所
    // ポイント追加
    public void AddPoint(int amount)
    {
        // ポイント加算
        currentExp += amount;

        // 100以上になったらレベルポイント獲得
        while (currentExp >= 10)
        {
            //// Spawn位置で演出生成
            //PointGetEffect effectInstance = Instantiate(
            //    pointEffectPrefab,
            //    pointSpawn.position,       // 出現場所
            //    Quaternion.identity,
            //    pointSpawn.parent          // 親は Canvas 内にしておく
            //);

            // ポイント獲得演出再生
            //effectInstance.PlayEffect();

            // 100消費
            currentExp -= 10;

            // レベルポイント獲得
            levelPoint++;

            SpawnExp();

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
    }

    // Exp生成用
    private void SpawnExp()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Instantiate(expPrefab,player.position,Quaternion.identity);
        }
    }
}
