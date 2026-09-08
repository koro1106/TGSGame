
using UnityEngine;
using UnityEngine.UI;

public class AmmoRecoverEffect : MonoBehaviour
{
    [Header("落下")]
    [SerializeField] float fallSpeed = 1000f;

    [Header("ぼよん")]
    [SerializeField] float squishAmount = 0.25f;
    [SerializeField] float squishTime = 0.08f;

    [Header("到着エフェクト")]
    [SerializeField] GameObject arriveEffectPrefab;

    [Header("エフェクト位置補正")]
    [SerializeField]
    Vector2 effectOffset = new Vector2(0f, -100f);

    [SerializeField] float effectLifeTime = 2.5f;

    private RectTransform rect;
    private Image image;

    private Vector3 targetPos;
    private RectTransform targetRect;

    private bool arrived = false;
    private System.Action onArrive;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        if (image == null)
        {
            Debug.LogError(
                "AmmoRecoverEffect に Image がありません。"
            );
        }
    }

    public void Init(
        Sprite sprite,
        Vector3 target,
        RectTransform targetUI,
        System.Action callback)
    {
        // =========================================
        // Spriteチェック
        // =========================================

        if (sprite == null)
        {
            Debug.LogError(
                "AmmoRecoverEffect に渡されたSpriteがnullです。"
            );

            Destroy(gameObject);
            return;
        }

        if (image == null)
        {
            Debug.LogError(
                "AmmoRecoverEffect の Image が取得できません。"
            );

            Destroy(gameObject);
            return;
        }

        // =========================================
        // ★重要
        // 回復する弾のSpriteを設定
        // =========================================

        image.sprite = sprite;
        image.enabled = true;
        image.preserveAspect = true;

        // =========================================
        // 情報保存
        // =========================================

        targetPos = target;
        targetRect = targetUI;
        onArrive = callback;

        // =========================================
        // 初期位置
        // =========================================

        rect.position =
            target + Vector3.up * 90f;

        rect.localScale =
            Vector3.one;

        arrived = false;

        // =========================================
        // 到着エフェクト
        // =========================================

        if (arriveEffectPrefab != null &&
            targetRect != null)
        {
            GameObject fx =
                Instantiate(arriveEffectPrefab);

            fx.transform.position =
                targetRect.position +
                new Vector3(
                    effectOffset.x,
                    effectOffset.y,
                    0f
                );

            Destroy(
                fx,
                effectLifeTime
            );
        }
    }

    void Update()
    {
        if (arrived)
            return;

        // =========================================
        // UIが動いても追従
        // =========================================

        if (targetRect != null)
        {
            targetPos =
                targetRect.position;
        }

        rect.position =
            Vector3.MoveTowards(
                rect.position,
                targetPos,
                fallSpeed * Time.deltaTime
            );

        float dist =
            Vector3.Distance(
                rect.position,
                targetPos
            );

        // =========================================
        // 到着
        // =========================================

        if (dist < 50f)
        {
            arrived = true;

            rect.position =
                targetPos;

            // =====================================
            // ★先に回復処理
            // =====================================

            onArrive?.Invoke();

            // =====================================
            // その後ぼよん
            // =====================================

            StartCoroutine(
                BounceAnimation()
            );
        }
    }

    System.Collections.IEnumerator BounceAnimation()
    {
        Vector3 normalScale =
            Vector3.one;

        Vector3 squishScale =
            new Vector3(
                1f + squishAmount,
                1f - squishAmount,
                1f
            );

        Vector3 startPos =
            rect.position;

        Vector3 squishPos =
            startPos +
            Vector3.down * 20f;

        float t = 0f;

        // =========================================
        // 潰れる
        // =========================================

        while (t < squishTime)
        {
            t += Time.deltaTime;

            float p =
                Mathf.Clamp01(
                    t / squishTime
                );

            p =
                Mathf.Sin(
                    p * Mathf.PI * 0.5f
                );

            rect.localScale =
                Vector3.Lerp(
                    normalScale,
                    squishScale,
                    p
                );

            rect.position =
                Vector3.Lerp(
                    startPos,
                    squishPos,
                    p
                );

            yield return null;
        }

        // =========================================
        // 戻る
        // =========================================

        t = 0f;

        while (t < squishTime * 1.5f)
        {
            t += Time.deltaTime;

            float p =
                Mathf.Clamp01(
                    t / (squishTime * 1.5f)
                );

            p =
                1f -
                Mathf.Pow(
                    1f - p,
                    3f
                );

            rect.localScale =
                Vector3.Lerp(
                    squishScale,
                    normalScale,
                    p
                );

            rect.position =
                Vector3.Lerp(
                    squishPos,
                    startPos,
                    p
                );

            yield return null;
        }

        rect.localScale =
            normalScale;

        rect.position =
            startPos;

        Destroy(gameObject);
    }
}
