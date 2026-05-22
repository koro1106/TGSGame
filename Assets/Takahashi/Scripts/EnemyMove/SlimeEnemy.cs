using UnityEngine;
using System.Collections;

public class SlimeEnemy : MonoBehaviour
{
    [Header("移動")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 180f;

    [Header("HP")]
    public int maxHP = 40;

    [Header("サイズ変化")]
    public float scaleSmooth = 12f;

    //========================

    private Vector2 moveDirection;

    private int currentHP;

    private Vector3 baseScale;
    private Vector3 targetScale;

    //========================
    // 拘束
    //========================

    private Coroutine bindCoroutine;
    private bool isBind = false;

    //========================
    // 開始
    //========================

    void Start()
    {
        currentHP = maxHP;

        baseScale = transform.localScale;
        targetScale = baseScale;

        // スポーン＆移動方向
        SetSpawnAndDirection();

        UpdateTargetScale();
    }

    //========================
    // 更新
    //========================

    void Update()
    {
        //========================
        // 拘束中停止
        //========================

        if (isBind)
        {
            return;
        }

        //========================
        // 移動
        //========================

        transform.Translate(
            moveDirection *
            moveSpeed *
            Time.deltaTime,
            Space.World
        );

        //========================
        // 回転
        //========================

        transform.Rotate(
            0,
            0,
            rotateSpeed *
            Time.deltaTime
        );

        //========================
        // サイズ変化
        //========================

        transform.localScale =
            Vector3.Lerp(
                transform.localScale,
                targetScale,
                Time.deltaTime *
                scaleSmooth
            );

        //========================
        // 画面外チェック
        //========================

        CheckOutOfScreen();
    }

    //========================
    // ダメージ
    //========================

    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            UpdateTargetScale();
        }
    }

    //========================
    // サイズ更新
    //========================

    void UpdateTargetScale()
    {
        float ratio =
            (float)currentHP / maxHP;

        targetScale =
            baseScale * ratio;
    }

    //========================
    // 死亡
    //========================

    void Die()
    {
        Destroy(gameObject);
    }

    //========================
    // 拘束開始
    //========================

    public void StartBind(float time)
    {
        if (bindCoroutine != null)
        {
            StopCoroutine(bindCoroutine);
        }

        bindCoroutine =
            StartCoroutine(
                BindCoroutine(time)
            );
    }

    //========================
    // 拘束処理
    //========================

    IEnumerator BindCoroutine(float time)
    {
        isBind = true;

        yield return new WaitForSeconds(time);

        isBind = false;

        bindCoroutine = null;
    }

    //========================
    // 画面外削除
    //========================

    void CheckOutOfScreen()
    {
        Camera cam = Camera.main;

        float height =
            cam.orthographicSize;

        float width =
            height * cam.aspect;

        Vector2 pos =
            transform.position;

        float margin = 100f;

        if (
            pos.x < -width - margin ||
            pos.x > width + margin ||
            pos.y < -height - margin ||
            pos.y > height + margin
        )
        {
            Destroy(gameObject);
        }
    }

    //========================
    // スポーン＆方向設定
    //========================

    void SetSpawnAndDirection()
    {
        Camera cam = Camera.main;

        float height =
            cam.orthographicSize;

        float width =
            height * cam.aspect;

        int side =
            Random.Range(0, 4);

        Vector2 spawnPos =
            Vector2.zero;

        //========================
        // 画面外スポーン
        //========================

        switch (side)
        {
            case 0:

                spawnPos =
                    new Vector2(
                        width + 1,
                        Random.Range(
                            -height,
                            height
                        )
                    );

                break;

            case 1:

                spawnPos =
                    new Vector2(
                        -width - 1,
                        Random.Range(
                            -height,
                            height
                        )
                    );

                break;

            case 2:

                spawnPos =
                    new Vector2(
                        Random.Range(
                            -width,
                            width
                        ),
                        height + 1
                    );

                break;

            case 3:

                spawnPos =
                    new Vector2(
                        Random.Range(
                            -width,
                            width
                        ),
                        -height - 1
                    );

                break;
        }

        transform.position =
            spawnPos;

        //========================
        // ランダム地点へ向かう
        //========================

        Vector2 target =
            new Vector2(
                Random.Range(
                    -width,
                    width
                ),
                Random.Range(
                    -height,
                    height
                )
            );

        moveDirection =
            (target - spawnPos)
            .normalized;
    }
}