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
    Vector2 effectOffset =
    new Vector2(0f, -100f);

    [SerializeField] float effectLifeTime = 2.5f;

    RectTransform rect;

    Vector3 targetPos;

    RectTransform targetRect;

    bool arrived = false;

    System.Action onArrive;

    bool effectPlayed = false;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Init(
     Sprite sprite,
     Vector3 target,
     RectTransform targetUI,
     System.Action callback)
    {
        GetComponent<Image>().sprite = sprite;

        targetPos = target;

        targetRect = targetUI;

        onArrive = callback;

        // 上から出現
        rect.position =
            target + Vector3.up * 90f;

        arrived = false;
        effectPlayed = false;
    }

    void Update()
    {
        if (arrived) return;

        // UIが動いても追従
        if (targetRect != null)
        {
            targetPos = targetRect.position;
        }

        rect.position = Vector3.MoveTowards(
            rect.position,
            targetPos,
            fallSpeed * Time.deltaTime
        );

        float dist = Vector3.Distance(rect.position, targetPos);

        // 少し手前でParticle表示
        if (!effectPlayed && dist < 5000f)
        {
            effectPlayed = true;

            if (arriveEffectPrefab != null && targetRect != null)
            {
                GameObject fx = Instantiate(arriveEffectPrefab);

                fx.transform.position =
                    targetRect.position +
                    new Vector3(effectOffset.x, effectOffset.y, 0f);

                Destroy(fx, effectLifeTime);
            }
        }

        // 到着
        if (dist < 50f)
        {
            arrived = true;

            rect.position = targetPos;

            onArrive?.Invoke();

            StartCoroutine(BounceAnimation());
        }
    }

    System.Collections.IEnumerator BounceAnimation()
    {
        Vector3 normalScale = Vector3.one;

        // 潰れ形
        Vector3 squishScale =
            new Vector3(
                1f + squishAmount,
                1f - squishAmount,
                1f
            );

        Vector3 startPos = rect.position;

        // 少し沈む
        Vector3 squishPos =
            startPos + Vector3.down * 20f;

        float t = 0f;

        //=====================
        // 潰れる
        //=====================

        while (t < squishTime)
        {
            t += Time.deltaTime;

            float p = t / squishTime;

            // 柔らかい感じ
            p = Mathf.Sin(p * Mathf.PI * 0.5f);

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

        t = 0f;

        //=====================
        // 戻る
        //=====================

        while (t < squishTime * 1.5f)
        {
            t += Time.deltaTime;

            float p = t / (squishTime * 1.5f);

            // ぷるん感
            p = 1f - Mathf.Pow(1f - p, 3f);

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

        //=====================
        // エフェクト
        //=====================

        if (arriveEffectPrefab != null && targetRect != null)
        {
            GameObject fx = Instantiate(arriveEffectPrefab);

            fx.transform.position =
                targetRect.position +
                new Vector3(effectOffset.x, effectOffset.y, 0f);

            Destroy(fx, effectLifeTime);

            //RectTransform fxRect =
            //    fx.GetComponent<RectTransform>();

            //if (fxRect != null)
            //{
            //    fxRect.position = targetRect.position;

            //    fxRect.anchoredPosition += effectOffset;

            //    fxRect.localScale = Vector3.one;
            //}

            Destroy(fx, effectLifeTime);
        }

        Destroy(gameObject);
    }
}