using UnityEngine;
using UnityEngine.UI;

public class AmmoRecoverEffect : MonoBehaviour
{
    [Header("落下")]
    [SerializeField] float fallSpeed = 1800f;

    [Header("ぼよん")]
    [SerializeField] float squishAmount = 0.25f;
    [SerializeField] float squishTime = 0.08f;

    [Header("到着エフェクト")]
    [SerializeField] GameObject arriveEffectPrefab;

    [SerializeField] float effectLifeTime = 2.5f;

    RectTransform rect;

    Vector3 targetPos;

    bool arrived = false;

    System.Action onArrive;

    public void Init(
        Sprite sprite,
        Vector3 target,
        System.Action callback)
    {
        rect = GetComponent<RectTransform>();

        GetComponent<Image>().sprite = sprite;

        targetPos = target;

        onArrive = callback;

        // 上からスタート
        rect.position =
            target + Vector3.up * 500f;
    }

    void Update()
    {
        if (arrived) return;

        rect.position = Vector3.MoveTowards(
            rect.position,
            targetPos,
            fallSpeed * Time.deltaTime
        );

        float dist =
            Vector3.Distance(rect.position, targetPos);

        if (dist < 5f)
        {
            arrived = true;

            rect.position = targetPos;

            onArrive?.Invoke();

            StartCoroutine(BounceAnimation());
        }
    }

    System.Collections.IEnumerator BounceAnimation()
    {
        Vector3 normal = Vector3.one;

        // 横に広がって下に沈む
        Vector3 squish =
            new Vector3(
                1f + squishAmount,
                1f - squishAmount,
                1f
            );

        float t = 0f;

        // 潰れる
        while (t < squishTime)
        {
            t += Time.deltaTime;

            rect.localScale =
                Vector3.Lerp(
                    normal,
                    squish,
                    t / squishTime
                );

            yield return null;
        }

        t = 0f;

        // 戻る
        while (t < squishTime)
        {
            t += Time.deltaTime;

            rect.localScale =
                Vector3.Lerp(
                    squish,
                    normal,
                    t / squishTime
                );

            yield return null;
        }

        //=====================
        // エフェクト生成
        //=====================

        if (arriveEffectPrefab != null)
        {
            GameObject fx =
                Instantiate(
                    arriveEffectPrefab,
                    targetPos,
                    Quaternion.identity,
                    transform.parent
                );

            Destroy(fx, effectLifeTime);
        }

        Destroy(gameObject);
    }
}