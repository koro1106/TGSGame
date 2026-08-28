using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Action = System.Action; // ★変更：Systemを丸ごと持ち込むとUnityEngine.RandomとSystem.Randomが衝突するため、Actionだけをエイリアスとして使う

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

    // ★追加：ダメージの属性（見た目の色分けに使用）
    public enum DamageAttribute
    {
        Normal,    // 通常（物理弾など）
        Poison,    // 毒（PoisonBullet）
        Thunder,   // 雷（仮実装：まだ弾は無いが色だけ用意）
        Explosion  // 爆発（仮実装：まだ弾は無いが色だけ用意）
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

    [Header("属性ダメージカラー")]
    public Color normalDamageColor = Color.white;
    public Color poisonDamageColor = new Color(0.6f, 0.2f, 0.8f);    // 紫
    public Color thunderDamageColor = new Color(0.4f, 0.9f, 1f);     // 水色（仮）
    public Color explosionDamageColor = new Color(1f, 0.15f, 0.15f); // 赤（仮）

    [Header("HPバー")]
    public Slider hpSlider; //hpバー
    public Slider hpDelaySlider; //ダメージを受けた時のhpばー

    [Header("HPバー：時計回りの減り方")]
    public bool useRadialFill = true; // ONにすると時計みたいな円形の減り方になる
    public Image.Origin180 radialOrigin = Image.Origin180.Top; // 弓形の上側を直径として使う
    public bool radialClockwise = false; // 右端から減って左端が最後に残る向き。逆だったらここを切り替える

    [Header("HPバー非表示設定")]
    public GameObject hpBarRoot; // HPバーをまとめている親オブジェクト（背景枠なども含む場合に使用。未設定ならスライダーを個別に非表示にします）

    [Header("HPバー出現アニメーション")]
    [Tooltip("HPバーが初めて表示される瞬間、一瞬これだけ拡大してから元のサイズに戻る（1=拡大なし）")]
    public float hpBarPopupScale = 1.3f;
    [Tooltip("出現時に一瞬これだけ傾いてから元の角度に戻る（度数。0=傾きなし）")]
    public float hpBarPopupTiltAngle = 12f;
    [Tooltip("拡大・傾き→元の状態に戻るまでの合計時間（秒）")]
    public float hpBarPopupDuration = 0.15f;

    private Coroutine hpBarPopupCoroutine;

    [Header("被弾点滅")]
    public Color hitFlashColor = Color.red; // 点滅させる色
    public float hitFlashDuration = 0.08f;  // 1回の点滅の長さ（秒）

    // 点滅させるSpriteRenderer（子も含めて全部）
    private SpriteRenderer[] srs;
    private Coroutine flashCoroutine; // 実行中の点滅コルーチン

    [Header("HPバーアニメーション設定")]
    public float hpBarSmoothSpeed = 4f; // （現在未使用：以前のLerp方式で使っていたパラメータ）
    public float hpBarDelayTime = 0.1f; // 残像バーが追従し始めるまでの待ち時間（秒）
    public float hpBarDelaySpeed = 2f;  // 残像バーが追いつく速さ（1秒あたりに動く割合。maxHPを掛けて実際のHP量に変換）

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

    [Header("死亡時のタグ変更（自動攻撃のターゲット外し用）")]
    public string deadTag = "Untagged"; // 死亡演出中に付け替えるタグ。専用タグを作るならそれを指定してください

    private bool hasTakenDamage = false; // 一度でも被弾したか（HPバー表示用）

    // ★追加：この敵が死亡した瞬間に発火するイベント。
    // 誰でも `enemyHP.OnDeath += 処理;` の形で購読できる。
    // ボスの場合はBossMove側でこれを購読し、EnemySpawner.BossDefeated()を呼ぶのに使う。
    public event Action OnDeath;

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

        // 被弾前はHPバーを隠しておく
        HideHPBar();
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

    // HPバーのアニメーション更新
    // メインバー：現在HPに即座に反映（本体はガクッと減る）
    // 残像バー：少し待ってから、一定速度で追いついていく（削れた量がじわっと見える）
    void UpdateHPBarAnimation()
    {
        // メインバーは遅延なしで即座に現在HPを反映
        displayedHP = currentHP;

        if (hpSlider != null)
        {
            hpSlider.value = displayedHP;
        }

        // 残像バー（遅れて追従）
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
            if (delayedHP > currentHP)
            {
                // ダメージを受けた場合：一定速度でゆっくり追いつく（Lerpではなく等速）
                delayedHP = Mathf.MoveTowards(
                    delayedHP, currentHP, hpBarDelaySpeed * maxHP * Time.deltaTime);
            }
            else
            {
                // 回復した場合などは即座に合わせる
                delayedHP = currentHP;
            }
        }

        if (hpDelaySlider != null)
        {
            hpDelaySlider.value = delayedHP;
        }
    }

    // ダメージ処理
    // attribute：ダメージの属性（Poison/Thunder/Explosionなど）。省略時はNormal扱い
    public void TakeDamage(int damage, bool isCritical = false, DamageAttribute attribute = DamageAttribute.Normal)
    {
        if (isDying) return;

        // 初回被弾でHPバーを表示する
        if (!hasTakenDamage)
        {
            hasTakenDamage = true;
            ShowHPBar();
        }

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

        ShowDamage(damage, isCritical, attribute); // ダメージ表示

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
            RemoveFromTargeting(); // 自動攻撃のタゲから即座に外す
            StartCoroutine(DeathSpiral()); // 死亡処理
        }
    }

    // HP増加処理
    void GrowHP()
    {
        maxHP = Mathf.RoundToInt(maxHP * growMultiplier);
        currentHP = Mathf.RoundToInt(currentHP * growMultiplier);

        currentHP = Mathf.Min(currentHP, maxHP);

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
            total += finalChance;

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
        RemoveFromTargeting(); // 自動攻撃のタゲから即座に外す
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
            hpBarRoot.SetActive(false);
        }
        else
        {
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

    // HPバーを表示する（初回被弾時に呼ばれる）
    // 表示と同時に、一瞬拡大してから元のサイズへ戻る「ポン」というポップアニメーションを再生する
    void ShowHPBar()
    {
        if (hpBarRoot != null)
        {
            hpBarRoot.SetActive(true);
            PlayHPBarPopup(hpBarRoot.transform);
        }
        else
        {
            if (hpSlider != null)
            {
                hpSlider.gameObject.SetActive(true);
                PlayHPBarPopup(hpSlider.transform);
            }

            if (hpDelaySlider != null)
            {
                hpDelaySlider.gameObject.SetActive(true);
            }
        }
    }

    // HPバー出現時のポップアニメーションを開始する
    void PlayHPBarPopup(Transform target)
    {
        if (target == null) return;

        if (hpBarPopupCoroutine != null)
        {
            StopCoroutine(hpBarPopupCoroutine);
        }

        hpBarPopupCoroutine = StartCoroutine(HPBarPopupRoutine(target));
    }

    // 一瞬 hpBarPopupScale 倍まで拡大＋hpBarPopupTiltAngle度傾く → 元の状態へ戻る、シンプルなポップアニメーション
    IEnumerator HPBarPopupRoutine(Transform target)
    {
        Vector3 baseLocalScale = target.localScale;
        Vector3 bigScale = baseLocalScale * hpBarPopupScale;

        Quaternion baseRot = target.localRotation;
        // 毎回同じ方向に傾くと単調なので、左右どちらかをランダムに選ぶ
        float tiltSign = (Random.value < 0.5f) ? 1f : -1f;
        Quaternion tiltRot = baseRot * Quaternion.Euler(0f, 0f, hpBarPopupTiltAngle * tiltSign);

        float half = Mathf.Max(hpBarPopupDuration * 0.5f, 0.0001f);
        float t = 0f;

        // 拡大＋傾く
        while (t < half)
        {
            t += Time.deltaTime;
            float ratio = t / half;
            target.localScale = Vector3.Lerp(baseLocalScale, bigScale, ratio);
            target.localRotation = Quaternion.Slerp(baseRot, tiltRot, ratio);
            yield return null;
        }

        t = 0f;

        // 元のサイズ・角度へ戻す
        while (t < half)
        {
            t += Time.deltaTime;
            float ratio = t / half;
            target.localScale = Vector3.Lerp(bigScale, baseLocalScale, ratio);
            target.localRotation = Quaternion.Slerp(tiltRot, baseRot, ratio);
            yield return null;
        }

        target.localScale = baseLocalScale;
        target.localRotation = baseRot;
        hpBarPopupCoroutine = null;
    }

    // 死亡確定時に即座に呼ぶ。コライダーを切り、タグを変更して
    // タグ検索型の自動攻撃のターゲットから即座に除外する
    void RemoveFromTargeting()
    {
        if (col != null)
        {
            col.enabled = false;
        }

        gameObject.tag = deadTag;
    }

    // 死亡演出
    // =========================================
    IEnumerator DeathSpiral()
    {
        isDying = true;

        OnDeath?.Invoke();

        EnemyMove move = GetComponent<EnemyMove>();
        if (move != null)
        {
            move.HideShadow();
        }

        HideHPBar();

        if (ComboManager.instance != null)
        {
            ComboManager.instance.AddCombo();
        }

        GunController gun = FindFirstObjectByType<GunController>();

        if (gun != null &&
            gun.recoverAmmoOnKill &&
            Random.value < gun.recoverAmmoChance / 100f)
        {
            gun.AddAmmo(gun.recoverAmmoAmount);
        }

        for (int i = 0; i < dropCount; i++)
        {
            GameObject drop = GetRandomDrop();

            if (drop != null)
            {
                int count = 1;

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

        Vector3 startScale = transform.localScale;

        float timer = 0f;

        while (timer < deathDuration)
        {
            timer += Time.deltaTime;

            float t = timer / deathDuration;

            transform.Rotate(
                0,
                0,
                rotationSpeed * Time.deltaTime
            );

            float radius = Mathf.Lerp(0.3f, 0f, t);

            Vector3 spiral = new Vector3(
                Mathf.Cos(timer * spiralSpeed),
                Mathf.Sin(timer * spiralSpeed),
                0
            ) * radius;

            transform.position += spiral * Time.deltaTime;

            float scale = Mathf.Lerp(1f, 0f, t);

            transform.localScale =
                startScale * (scale * scale);

            yield return null;
        }

        Destroy(gameObject);
    }

    // 被弾点滅（赤く一瞬光って元の色に戻る）
    IEnumerator HitFlash()
    {
        foreach (SpriteRenderer sr in srs)
        {
            if (sr != null)
            {
                sr.color = hitFlashColor;
            }
        }

        yield return new WaitForSeconds(hitFlashDuration);

        foreach (SpriteRenderer sr in srs)
        {
            if (sr != null)
            {
                sr.color = Color.white;
            }
        }
    }
    // ダメージUI表示
    // attribute：色分けに使用する属性
    void ShowDamage(int damage, bool isCritical, DamageAttribute attribute = DamageAttribute.Normal)
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

            if (isCritical)
            {
                dmg.SetCritical(); // クリティカルなら黄色＋アイコン（属性色で上書きしない）
            }
            else
            {
                //  属性に応じた色を設定（クリティカルでない時だけ）
                dmg.SetColor(GetDamageColor(attribute));
            }
        }
    }

    // 属性ごとのダメージ色を返す
    Color GetDamageColor(DamageAttribute attribute)
    {
        switch (attribute)
        {
            case DamageAttribute.Poison:
                return poisonDamageColor;
            case DamageAttribute.Thunder:
                return thunderDamageColor;
            case DamageAttribute.Explosion:
                return explosionDamageColor;
            default:
                return normalDamageColor;
        }
    }
}