using UnityEngine;

public class ComboManager : MonoBehaviour
{
    // どこからでも使えるようにする（シングルトン）
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
        // シングルトンのinstance登録
        instance = this;
    }

    // コンボ追加（外部から呼び出す）
    public void AddCombo()
    {
        // コンボ数+1
        combo++;

        // コンボ文字を表示
        ShowComboPopup();
    }

    // コンボ文字を表示する
    void ShowComboPopup()
    {
        // playerかPrefabが無ければ終了
        if (player == null || comboPopupPrefab == null)
            return;

        // 表示位置・背景回転角度・方向インデックスをセットで管理
        // offset    : プレイヤーからの相対座標
        // rotation  : 背景画像のみに適用する回転角度（Z軸）
        // direction : テキスト位置切り替え用（0=左、1=右、2=上）
        (Vector3 offset, float rotation, int direction)[] spawnData =
        {
            // 左 → Vector3.leftにマイナスをつけると右になるので修正
            (Vector3.left  * 120f,   0f, 0),  // 左 → そのまま
            (Vector3.right * 120f, -100f, 1),  // 右 → -100°
            (Vector3.up    * 145f,  -60f, 2),  // 上 → -60°
        };

        // 0〜2をランダム取得
        int index = Random.Range(0, spawnData.Length);

        // Player位置 + ランダム位置
        Vector3 spawnPos = player.position + spawnData[index].offset;

        // Prefabを回転なしで生成（ルートは回転させない）
        GameObject obj = Instantiate(
            comboPopupPrefab,
            spawnPos,
            Quaternion.identity
        );

        // ComboPopupコンポーネントを取得
        ComboPopup popup = obj.GetComponent<ComboPopup>();

        // あればコンボ数・背景回転・方向をセット
        if (popup != null)
        {
            popup.SetCombo(combo, spawnData[index].rotation, spawnData[index].direction);
        }
    }
}