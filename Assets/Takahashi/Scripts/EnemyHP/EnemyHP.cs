using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyHP : MonoBehaviour
{
    // ドロップアイテム設定用クラス
    [System.Serializable]
    public class DropItem
    {
        public GameObject prefab; // ドロップするPrefab

        [Range(0f, 100f)]
        public int chance; // ドロップ確率
    }

    [Header("HP")]
    public int maxHP = 100;      // 最大HP
    public int currentHP;        // 現在HP

    [Header("レア敵設定")]
    public bool isRareEnemy = false; //レア敵
    [Header("見た目")]
    public float scaleSmooth = 12f; // スケール補間速度

    [Header("時間経過HP増加")]
    public float growInterval = 10f;    // HP増加間隔
    public float growMultiplier = 1.5f; // HP増加倍率

    private float growTimer = 0f; // 経過時間

    [Header("死亡演出")]
    public float deathDuration = 1.2f;   // 死亡演出時間
    public float spiralSpeed = 20f;      // 渦スピード
    public float rotationSpeed = 1080f;  // 回転速度

    [Header("ドロップ")]
    public DropItem[] dropItems;
    public int dropCount = 1;

    [Header("ダメージ表示")]
    public GameObject damageText; // ダメージUI

    [Header("HPバー")]
    public Slider hpSlider; //hpバー
    public Slider hpDelaySlider; //ダメージを受けた時のhpばー

    [Header("HPバー：時計回りの減り方")]
    public bool useRadialFill = true; // ONにすると時計みたいな円形の減り方になる
    public Image.Origin180 radialOrigin = Image.Origin180.Top; // 弓形の上側を直径として使う
    public bool radialClockwise = false; // 右端から減って左端が最後に残る向き。逆だったらここを切り替える

    [Header("HPバー非表示設定")]
    public GameObject hpBarRoot; // HPバーをまとめている親オブジェクト（背景枠なども含む場合に使用。未設定ならスライダーを個別に非表示にします）

    [Header("被弾点滅")]
    public Color hitFlashColor = Color.red; // 点滅させる色
    public float hitFlashDuration = 0.08f;  // 1回の点滅の長さ（秒）

    // 点滅させるSpriteRenderer（子も含めて全部）
    private SpriteRenderer[] srs;
    private Coroutine flashCoroutine; // 実行中の点滅コルーチン

    [Header("HPバーアニメーション設定")]
    public float hpBarSmoothSpeed = 4f; // メインバーが現在HPに追いつく速さ
    public float hpBarDelayTime = 0.1f; // 残像バーが追従し始めるまでの待ち時間（秒）
    public float hpBarDelaySpeed = 2f;  // 残像バーが追いつく速さ

    private float displayedHP;     // メインバーが今表示している値（アニメ用）
    private float delayedHP;       // 残像バーが今表示している値（アニメ用）
    private float delayTimer = 0f; // 残像バーの待ち時間カウント
    private bool delayWaiting = false; // 残像バーが「まだ待っている」状態かどうか

    private Vector3 baseScale;   // 初期スケール
    private Vector3 targetScale; // 目標スケール

    private bool isDying = false; // 死亡中フラグ
    private Collider2D col;       // コライダー

    private bool isBind = false;          // 鎖などで拘束されている間true
    private Coroutine bindCoroutine;      // 実行中のバインドコルーチン

    private IHitSlowable hitSlowable; // 被弾鈍化を呼ぶための参照。EnemyMove/EnemyWarpMoveどちらでも対応

    public PlayerStats stats; // プレイヤーステータス
    void Start()
    {
        // 移動スクリプト取得（被弾時の鈍化呼び出し用）
        hitSlowable = GetComponent<IHitSlowable>();

        // HP初期化
        currentHP = maxHP;

        // HPバーアニメーション用の値も初期化（最初はピッタリ満タンの状態）
        displayedHP = currentHP;
        delayedHP = currentHP;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
            SetupRadialFill(hpSlider);
        }

        if (hpDelaySlider != null)
        {
            hpDelaySlider.maxValue = maxHP;
            hpDelaySlider.value = currentHP;
            SetupRadialFill(hpDelaySlider);
        }

        // スケール保存
        baseScale = transform.localScale;
        targetScale = baseScale;

        // コライダー取得
        col = GetComponent<Collider2D>();

        // 点滅用のスプライトレンダラー取得
        // 点滅用（子オブジェクトも含めて取得）
        srs = GetComponentsInChildren<SpriteRenderer>();
    }
        void Update()
        {
            // 拘束中（鎖など）は何もしない
            if (isBind)
            {
                return;
            }

            // 死亡中は処理しない
            if (isDying) return;

            // スケール反映

            //transform.localScale = Vector3.Lerp(
            //    transform.localScale,
            //    targetScale,
            //    Time.deltaTime * scaleSmooth
            //);

            // HPバーのアニメーション更新
            UpdateHPBarAnimation();
        }
    
    // HPバーのアニメーション処理
    // メインバー：なめらかに現在HPへ追従
    // 残像バー：少し待ってから、ゆっくり追従（削れた量がじわっと見える）
    void UpdateHPBarAnimation()
    {
        // メインバー（即時に近いがなめらか）
        displayedHP = Mathf.Lerp(displayedHP, currentHP, Time.deltaTime * hpBarSmoothSpeed);

        if (hpSlider != null)
        {
            hpSlider.value = displayedHP;
        }

        //　残像バー（遅れて追従
        if (delayWaiting)
        {
            // ダメージ直後はまだ動かさず、一定時間待つ
            delayTimer += Time.deltaTime;

            if (delayTimer >= hpBarDelayTime)
            {
                delayWaiting = false; // 待ち終わったので追従開始
            }
        }
        else
        {
            delayedHP = Mathf.Lerp(delayedHP, currentHP, Time.deltaTime * hpBarDelaySpeed);
        }

        if (hpDelaySlider != null)
        {
            hpDelaySlider.value = delayedHP;
        }
    }
    // ダメージ処理
    public void TakeDamage(int damage, bool isCritical = false)
    {
        if (isDying) return;

        //レア敵は１ダメ
        if (isRareEnemy)
        {
            damage = 1;
        }

        currentHP -= damage;              // HP減少
        currentHP = Mathf.Max(currentHP, 0); // 0以下防止

        // HPバーアニメーション開始
        delayWaiting = true;
        delayTimer = 0f;

        //if (hpSlider != null)
        //{
        //    hpSlider.value = currentHP;
        //}

        ShowDamage(damage, isCritical); // ダメージ表示
                                        //UpdateScale();      // 見た目更新

        // 被弾点滅
        // 被弾点滅
        if (srs.Length > 0)
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(HitFlash());
        }

        // 被弾鈍化（鈍化対応の移動スクリプトがアタッチされている場合のみ）
        if (hitSlowable != null)
        {
            hitSlowable.ApplyHitSlow();
        }

        if (currentHP <= 0)
        {
            StartCoroutine(DeathSpiral()); // 死亡処理
        }
    }

    // HP割合でサイズ変更
   /* void UpdateScale()
    {
        float ratio = 1f;

        if (currentHP > maxHP * 0.75f)
            ratio = 1f;
        else if (currentHP > maxHP * 0.5f)
            ratio = 0.875f;
        else if (currentHP > maxHP * 0.25f)
            ratio = 0.75f;
        else
            ratio = 0.625f;

        targetScale = baseScale * ratio;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }
    }*/

    // HP増加処理
    void GrowHP()
    {
        maxHP = Mathf.RoundToInt(maxHP * growMultiplier);
        currentHP = Mathf.RoundToInt(currentHP * growMultiplier);

        currentHP = Mathf.Min(currentHP, maxHP);

        //UpdateScale();

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }
    }

    // ランダムドロップ抽選
    GameObject GetRandomDrop()
    {
        float rand = Random.Range(0f, 100f);
        float total = 0f;

        foreach (DropItem item in dropItems)
        {
            float finalChance = item.chance + stats.expDroprate;
            total += item.chance;

            if (rand <= total)
            {
                return item.prefab;
            }
        }

        return null;
    }
    public void ForceKill()
    {
        if (isDying) return;

        currentHP = 0;
        StartCoroutine(DeathSpiral());
    }

    // 鎖などで一定時間拘束する（BindBulletから呼ばれる）
    public void StartBind(float time)
    {
        if (bindCoroutine != null)
        {
            StopCoroutine(bindCoroutine);
        }

        bindCoroutine = StartCoroutine(BindCoroutine(time));
    }

    IEnumerator BindCoroutine(float time)
    {
        isBind = true;

        yield return new WaitForSeconds(time);

        isBind = false;
        bindCoroutine = null;
    }

    // 拘束中かどうか（他スクリプトからの参照用）
    public bool IsBind()
    {
        return isBind;
    }

    // SliderのFill画像を「時計みたいに円形で減る」設定に自動構成する
    // ※Image Type を Filled にしておけば、Sliderはvalueの変化に合わせて
    //   自動でfillAmountを更新してくれるので、TakeDamage側のコードは変更不要
    void SetupRadialFill(Slider slider)
    {
        if (!useRadialFill) return;
        if (slider.fillRect == null) return;

        Image fillImage = slider.fillRect.GetComponent<Image>();
        if (fillImage == null) return;

        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Radial180;
        fillImage.fillOrigin = (int)radialOrigin;
        fillImage.fillClockwise = radialClockwise;
    }

    // HPバーを非表示にする
    void HideHPBar()
    {
        if (hpBarRoot != null)
        {
            // 親オブジェクトが設定されていればまとめて非表示
            hpBarRoot.SetActive(false);
        }
        else
        {
            // 設定されていなければスライダーを個別に非表示
            if (hpSlider != null)
            {
                hpSlider.gameObject.SetActive(false);
            }

            if (hpDelaySlider != null)
            {
                hpDelaySlider.gameObject.SetActive(false);
            }
        }
    }

    // 死亡演出
    // =========================================
    IEnumerator DeathSpiral()
    {
        // 死亡フラグON
        isDying = true;

        // 影をすぐ消す
        EnemyMove move = GetComponent<EnemyMove>();
        if (move != null)
        {
            move.HideShadow();
        }

        // 当たり判定OFF
        if (col != null)
        {
            col.enabled = false;
        }

        // HPバーを非表示
        HideHPBar();

        //コンボ追加
        if (ComboManager.instance != null)
        {
            ComboManager.instance.AddCombo();
        }

        // =========================
        // ドロップ生成
        // =========================
        for (int i = 0; i < dropCount; i++)
        {
            // 抽選
            GameObject drop = GetRandomDrop();

            // nullじゃなければ生成
            if (drop != null)
            {
                int count = 1;

                // 20%で倍率発動
                if (Random.Range(0f, 100f) < 50f)
                {
                    count = stats.expDroprateDouble;
                }

                for (int j = 0; j < count; j++)
                {
                    Vector3 offset = new Vector3(
                        Random.Range(-50f, 50f),
                        Random.Range(-50f, 50f),
                        0
                    );

                    Instantiate(
                        drop,
                        transform.position + offset,
                        Quaternion.identity
                    );
                }
            }
        }

        // =========================
        // 死亡アニメーション
        // =========================
        Vector3 startScale = transform.localScale;

        float timer = 0f;

        while (timer < deathDuration)
        {
            timer += Time.deltaTime;

            float t = timer / deathDuration;

            // 回転
            transform.Rotate(
                0,
                0,
                rotationSpeed * Time.deltaTime
            );

            // 渦移動
            float radius = Mathf.Lerp(0.3f, 0f, t);

            Vector3 spiral = new Vector3(
                Mathf.Cos(timer * spiralSpeed),
                Mathf.Sin(timer * spiralSpeed),
                0
            ) * radius;

            transform.position += spiral * Time.deltaTime;

            // 縮小
            float scale = Mathf.Lerp(1f, 0f, t);

            transform.localScale =
                startScale * (scale * scale);

            yield return null;
        }

        // 完全消滅
        Destroy(gameObject);
    }
    // 被弾点滅（赤く一瞬光って元の色に戻る）
   IEnumerator HitFlash()
{
    // 全部赤くする
    foreach (SpriteRenderer sr in srs)
    {
        if (sr != null)
        {
            sr.color = hitFlashColor;
        }
    }

    yield return new WaitForSeconds(hitFlashDuration);

    // 元の色（白）に戻す
    foreach (SpriteRenderer sr in srs)
    {
        if (sr != null)
        {
            sr.color = Color.white;
        }
    }
}
    // ダメージUI表示
    void ShowDamage(int damage, bool isCritical)
    {
        if (damageText == null) return;

        GameObject obj = Instantiate(
            damageText,
            transform.position,
            Quaternion.identity
        );

        DamageText dmg = obj.GetComponent<DamageText>();

        if (dmg != null)
        {
            dmg.SetDamage(damage);
            //クリティカルなら特別表示
            if (isCritical)
            {
                dmg.SetCritical();
            }
        }

    }
}