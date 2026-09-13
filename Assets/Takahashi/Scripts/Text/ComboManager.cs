using UnityEngine;

public class ComboManager : MonoBehaviour
{
    // どこからでも使えるようにする（シングルトン）
    public static ComboManager instance;

    [Header("Player")]
    // プレイヤーのTransform（未設定、または参照ミスの場合はタグ"Player"から自動取得）
    // ★変更：表示位置の基準としては使わなくなったが、
    //   AddCombo()が引数無しで呼ばれた場合のフォールバック用に残す
    public Transform player;

    [Header("コンボ表示")]
    // 表示用Prefab
    public GameObject comboPopupPrefab;

    // 現在のコンボ数
    private int combo = 0;

    // 外部（EnemySpawnerなど）からコンボ数を読み取れるようにする
    public int Combo => combo;

    void Awake()
    {
        // シングルトンのinstance登録
        instance = this;

        // playerが未設定、または中身が空になっている場合はタグ"Player"から自動取得
        // → Inspectorで別オブジェクトを参照していて「中央に固定される」ような事故を防ぐ
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
            }
            else
            {
                Debug.LogWarning("ComboManager: player未設定で、タグ\"Player\"のオブジェクトも見つかりませんでした。");
            }
        }
    }

    // =========================================================
    // ★変更：コンボ追加（外部から呼び出す）
    // 倒した敵の位置を渡してもらうことで、その場所にコンボ数を表示する。
    // 引数を省略した場合は従来通りplayerの位置に表示される（互換性のため）。
    // =========================================================
    public void AddCombo(Vector3? sourcePosition = null)
    {
        // コンボ数+1
        combo++;

        // 表示位置を決定：渡された位置があればそれを使い、無ければplayerの位置にフォールバック
        Vector3 basePos = sourcePosition ?? (player != null ? player.position : transform.position);

        // コンボ文字を表示
        ShowComboPopup(basePos);
    }

    // コンボ数をリセットする（ボス出現時・撃破時などに使用）
    public void ResetCombo()
    {
        combo = 0;
    }

    // =========================================================
    // ★変更：コンボ文字を表示する
    // 表示の基準位置(basePos)を外部から受け取るようにした
    // （以前はplayer.positionに固定していた）
    // =========================================================
    void ShowComboPopup(Vector3 basePos)
    {
        // Prefabが無ければ終了
        if (comboPopupPrefab == null)
            return;

        // 表示位置・背景回転角度・方向インデックスをセットで管理
        // offset    : basePos（倒した敵の位置）からの相対座標
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

        // ★変更：player.position → basePos（倒した敵の位置）を基準に生成
        Vector3 spawnPos = basePos + spawnData[index].offset;

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