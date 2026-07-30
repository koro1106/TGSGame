using System.Collections;
using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    private TextMeshPro text;

    [Header("画面スケール調整")]
    [Tooltip("プロジェクトの座標スケール基準値。画面サイズが500想定なので500がデフォルト。" +
             "スケールが変わったらここだけ変えれば下の距離が自動で比例調整される")]
    public float screenScale = 500f;
    private float ScaleFactor => screenScale / 500f;

    [Header("見た目")]
    public int baseFontSize = 100;
    [Range(0f, 1f)]
    public float baseAlpha = 0.85f; // ほんの少し半透明にする

    [Header("出現位置（本来のTransformよりどれだけ上に出すか）")]
    public float spawnHeightOffset = 80f; // ※screenScale=500基準の値

    [Header("出現方向（左斜め上／上／右斜め上からランダム）")]
    [Tooltip("斜め方向の横のブレ幅の比率。0だと真上との差が無くなる")]
    public float diagonalSpread = 0.6f;

    [Header("① 下から出て（スポーン位置は本来の位置より少し下）")]
    public float spawnDropDistance = 15f; // ※screenScale=500基準の値
    public float spawnScale = 0.3f;       // 出始めの小ささ

    [Header("② 少し上がって、大きさ最大")]
    public float riseUpDuration = 0.10f;
    public float riseUpDistance = 20f;    // ※screenScale=500基準の値
    public float overshootScale = 1.3f;   // 一番大きくなるときのスケール

    [Header("③ 少し小さくなって（落ち着く）")]
    public float settleDuration = 0.12f;
    public float settleDistance = 8f;     // ※screenScale=500基準の値
    public float settleScale = 1.05f;     // 落ち着いた後のスケール

    [Header("④ 緩急で早くなり、小さくなり消える")]
    public float finalDuration = 0.35f;
    public float finalMoveDistance = 60f; // ※screenScale=500基準の値

    [Header("クリティカル")]
    public float criticalScaleMultiplier = 1.4f;

    private bool isCritical = false;
    private Vector2 direction;

    void Awake()
    {
        text = GetComponent<TextMeshPro>();
    }

    public void SetDamage(int damage)
    {
        text.text = damage.ToString();

        // 通常色（少し半透明）
        Color c = Color.white;
        c.a = baseAlpha;
        text.color = c;

        text.fontSize = baseFontSize;
        text.fontStyle = FontStyles.Bold; // 常に太字

        /* ダメージごとに色変更
        if (damage < 11)
        {
            text.color = Color.white; // 小ダメージ
        }
        else if (damage < 30)
        {
            text.color = Color.yellow; // 中ダメージ
        }
        else
        {
            text.color = Color.red; // 大ダメージ
        }*/
    }

    // クリティカル表示
    public void SetCritical()
    {
        isCritical = true;

        Color c = new Color(1f, 0.5f, 0f);
        c.a = baseAlpha;
        text.color = c;

        text.fontStyle = FontStyles.Bold;
        // サイズ倍率はPlayAnimation側でcriticalScaleMultiplierとして反映
    }

    void Start()
    {
        // 左斜め上／真上／右斜め上からランダムに1つ選ぶ
        int r = Random.Range(0, 3);
        if (r == 0) direction = new Vector2(-diagonalSpread, 1f).normalized;      // 左斜め上
        else if (r == 1) direction = Vector2.up;                                  // 真上
        else direction = new Vector2(diagonalSpread, 1f).normalized;              // 右斜め上

        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        float scale = ScaleFactor;

        // 本来出したい位置（敵の位置より少し上）
        Vector3 finalPosition = transform.position + new Vector3(0f, spawnHeightOffset * scale, 0f);

        float critMul = isCritical ? criticalScaleMultiplier : 1f;
        float t;

        // --- ① 下から出て（開始位置＝本来の位置より少し下、小さいサイズ） ---
        Vector3 startPos = finalPosition - (Vector3)(direction * (spawnDropDistance * scale));
        transform.position = startPos;
        transform.localScale = Vector3.one * (spawnScale * critMul);

        // --- ② 少し上がって、大きさ最大（オーバーシュート） ---
        Vector3 peakPos = startPos + (Vector3)(direction * (riseUpDistance * scale));
        t = 0f;
        while (t < riseUpDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / riseUpDuration);
            // イーズアウトで勢いよく飛び出す感じに
            float eased = 1f - (1f - p) * (1f - p);

            transform.position = Vector3.Lerp(startPos, peakPos, eased);
            float s = Mathf.Lerp(spawnScale, overshootScale, eased);
            transform.localScale = Vector3.one * (s * critMul);

            yield return null;
        }
        transform.position = peakPos;
        transform.localScale = Vector3.one * (overshootScale * critMul);

        // --- ③ 少し小さくなって落ち着く ---
        Vector3 settlePos = peakPos + (Vector3)(direction * (settleDistance * scale));
        t = 0f;
        while (t < settleDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / settleDuration);

            transform.position = Vector3.Lerp(peakPos, settlePos, p);
            float s = Mathf.Lerp(overshootScale, settleScale, p);
            transform.localScale = Vector3.one * (s * critMul);

            yield return null;
        }
        transform.position = settlePos;
        transform.localScale = Vector3.one * (settleScale * critMul);

        // --- ④ 緩急で早くなり、小さくなり消える（イーズインで加速） ---
        Vector3 endPos = settlePos + (Vector3)(direction * (finalMoveDistance * scale));
        Color startColor = text.color;
        t = 0f;
        while (t < finalDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / finalDuration);
            // イーズインで最初ゆっくり、だんだん速く
            float eased = p * p;

            transform.position = Vector3.Lerp(settlePos, endPos, eased);

            float s = Mathf.Lerp(settleScale, 0f, eased);
            transform.localScale = Vector3.one * (s * critMul);

            Color c = startColor;
            c.a = Mathf.Lerp(baseAlpha, 0f, eased);
            text.color = c;

            yield return null;
        }

        Destroy(gameObject);
    }
}