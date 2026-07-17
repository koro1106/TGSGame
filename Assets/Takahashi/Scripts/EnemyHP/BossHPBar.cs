using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class BossHPBar : MonoBehaviour
{
    [Header("UI参照")]
    public GameObject barRoot;
    public Image emptyImage;
    public Slider maxHpSlider;
    public Slider damageHpSlider;
    public Text bossNameText;

    [Header("演出設定")]
    public float expandDuration = 0.4f;   // 中央から左右に広がる時間
    public float fillDuration = 1.0f;     // 0→maxまで段々満ちる時間
    public float damageFollowSpeed = 2f;

    // ※位置移動(スライドイン)は廃止したため hiddenPosY / shownPosY は使用していません

    private EnemyHP targetHP;
    private Coroutine currentRoutine;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = barRoot.GetComponent<RectTransform>();
        if (barRoot != null)
            barRoot.SetActive(false);
    }

    void Update()
    {
        if (targetHP != null && targetHP.currentHP <= 0)
        {
            Hide();
        }
    }

    // ===== 予告表示メソッド =====
    // onComplete：演出（広がる→段々満タン）が全部終わった瞬間に呼ばれる処理。
    // EnemySpawner側から SpawnBoss を渡すことで「演出後にボス出現」を実現している。
    public void ShowWarning(string bossName, Action onComplete = null)
    {
        targetHP = null;

        if (barRoot != null)
            barRoot.SetActive(true);

        if (bossNameText != null)
            bossNameText.text = bossName;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ExpandAndFill(onComplete));
    }

    // ===== ボス出現時に呼ぶメソッド =====
    // ShowWarning() で既に演出は完了済みの前提。HPを紐付けて追従を開始するだけ。
    public void AttachBoss(EnemyHP hp)
    {
        targetHP = hp;

        if (barRoot != null)
            barRoot.SetActive(true);

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(TrackHP());
    }

    // ===== 予告演出を使わず、即座に表示＋HP紐付けしたい場合用（保険で残置） =====
    public void ShowBoss(EnemyHP hp, string bossName = "BOSS")
    {
        targetHP = hp;

        if (barRoot != null)
            barRoot.SetActive(true);

        if (bossNameText != null)
            bossNameText.text = bossName;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ExpandAndFill(null));
    }

    public void Hide()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        targetHP = null;

        if (barRoot != null)
            barRoot.SetActive(false);
    }

    // ===== 中央から左右に広がる → 少し間 → 段々満タン → 完了コールバック =====
    IEnumerator ExpandAndFill(Action onComplete)
    {
        maxHpSlider.value = 0f;
        damageHpSlider.value = 0f;

        // 位置は固定のまま。開始時は横幅0（縦の線状態）にしておく。
        // ※barRootのPivot.Xが0.5（中央）になっている必要があります。
        Vector3 scale = rectTransform.localScale;
        scale.x = 0f;
        rectTransform.localScale = scale;

        float t = 0f;
        while (t < expandDuration)
        {
            t += Time.deltaTime;
            float p = t / expandDuration;
            // オーバーシュートしない、なめらかに減速するイージングを使用
            float eased = EaseOutCubic(p);
            scale.x = eased;
            rectTransform.localScale = scale;
            yield return null;
        }
        scale.x = 1f;
        rectTransform.localScale = scale;

        // 少し間を置いてからHPが満ちる（登場→溜め→満タンの間を作る）
        yield return new WaitForSeconds(0.15f);

        // fillDuration秒かけて段々満タンにする
        float ft = 0f;
        while (ft < fillDuration)
        {
            ft += Time.deltaTime;
            float v = Mathf.Lerp(0f, 1f, ft / fillDuration);
            maxHpSlider.value = v;
            damageHpSlider.value = v;
            yield return null;
        }
        maxHpSlider.value = 1f;
        damageHpSlider.value = 1f;

        // 演出が完全に終わったのでコールバック実行（EnemySpawner側でボス出現に使う）
        onComplete?.Invoke();
    }

    IEnumerator TrackHP()
    {
        while (targetHP != null)
        {
            float ratio = (float)targetHP.currentHP / targetHP.maxHP;

            maxHpSlider.value = ratio;

            if (damageHpSlider.value > ratio)
            {
                damageHpSlider.value = Mathf.MoveTowards(
                    damageHpSlider.value, ratio, damageFollowSpeed * Time.deltaTime);
            }
            else
            {
                damageHpSlider.value = ratio;
            }

            yield return null;
        }
    }

    // オーバーシュートしない、なめらかに減速するイージング（現在の広がり演出で使用中）
    float EaseOutCubic(float x)
    {
        return 1f - Mathf.Pow(1f - x, 3f);
    }

    // 少しオーバーシュートしてから収まる、弾むようなイージング（現在は未使用）
    float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}