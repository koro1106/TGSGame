using UnityEngine;
using System.Collections;

public class EnemyWarpMove : MonoBehaviour
{
    [Header("移動")]
    public float moveSpeed = 3f;

    [Header("ワープ")]
    public float warpInterval = 3f;
    public float warpRange = 6f;
    public float warpAnimTime = 0.3f;

    private bool isWarping = false;
    private Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;

        StartCoroutine(WarpRoutine());
    }

    void Update()
    {
        if (isWarping) return;

        // 画面中央へ移動
        Vector3 dir = (-transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    IEnumerator WarpRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(warpInterval);

            yield return StartCoroutine(Warp());
        }
    }

    IEnumerator Warp()
    {
        isWarping = true;

        float timer = 0f;

        // 消える
        while (timer < warpAnimTime)
        {
            timer += Time.deltaTime;

            transform.localScale =
                Vector3.Lerp(startScale, Vector3.zero, timer / warpAnimTime);

            yield return null;
        }

        // ワープ
        Vector2 pos = Random.insideUnitCircle.normalized * warpRange;
        transform.position = pos;

        timer = 0f;

        // 出現
        while (timer < warpAnimTime)
        {
            timer += Time.deltaTime;

            transform.localScale =
                Vector3.Lerp(Vector3.zero, startScale, timer / warpAnimTime);

            yield return null;
        }

        transform.localScale = startScale;
        isWarping = false;
    }
}