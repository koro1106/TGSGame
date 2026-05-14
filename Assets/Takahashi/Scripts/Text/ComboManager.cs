using UnityEngine;

public class ComboManager : MonoBehaviour
{
    // どこからでも使えるようにする
    public static ComboManager instance;

    [Header("Player")]
    // プレイヤーのTransform
    public Transform player;

    [Header("コンボ表示")]
    // 表示用Prefab
    public GameObject comboPopupPrefab;

    // 現在のコンボ数
    private int combo = 0;

    void Awake()
    {
        // instance登録
        instance = this;
    }

    // コンボ追加
    public void AddCombo()
    {
        // コンボ数+1
        combo++;

        // 表示
        ShowComboPopup();
    }

    // コンボ文字を表示
    void ShowComboPopup()
    {
        // playerかPrefabが無ければ終了
        if (player == null || comboPopupPrefab == null)
            return;

        // 表示位置
        // 上・左・右
        Vector3[] positions =
        {
            Vector3.up * 50f,
            Vector3.left * 150f,
            Vector3.right * 150f
        };

        // 0〜2をランダム取得
        int index = Random.Range(0, positions.Length);

        // Player位置 + ランダム位置
        Vector3 spawnPos = player.position + positions[index];

        // Prefab生成
        GameObject obj = Instantiate(
            comboPopupPrefab,
            spawnPos,
            Quaternion.identity
        );

        // ComboPopup取得
        ComboPopup popup = obj.GetComponent<ComboPopup>();

        // あればコンボ数セット
        if (popup != null)
        {
            popup.SetCombo(combo);
        }
    }
}