using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("移動速度")]
    public float moveSpeed = 5f;

    [Header("追従するクロスヘア(UI)")]
    public RectTransform crosshair;

    [Header("クロスヘアがあるCanvas")]
    public Canvas canvas;

    [Header("移動するシーン名")]
    public string gameOverSceneName;

    [Header("リザルト後に戻る位置")]
    public Transform startPoint;

    public GunController gunController;

    private Rigidbody2D rb;
    private Camera cam;

    // リザルト復帰直後の移動を1回だけ止める
    private bool skipNextMove = false;

    private bool isDead = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }


    // =========================================================
    // Playerを開始位置へ戻す
    // =========================================================

    public void ResetPlayerPosition()
    {
        Debug.Log("★★★★★ ResetPlayerPosition 呼ばれた ★★★★★");

        if (startPoint == null)
        {
            Debug.LogError(
                "★★★★★ startPoint が NULL です ★★★★★"
            );
            return;
        }

        Vector2 startPos = startPoint.position;

        Debug.Log(
            "★★★★★ 戻す位置 : " + startPos + " ★★★★★"
        );

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = true;
            rb.position = startPos;
        }

        transform.position = new Vector3(
            startPos.x,
            startPos.y,
            transform.position.z
        );

        skipNextMove = true;
    }


    void FixedUpdate()
    {
        if (skipNextMove)
        {
            skipNextMove = false;
            return;
        }

        MoveToCrosshair();
    }


    // =========================================================
    // クロスヘアへ移動
    // =========================================================

    void MoveToCrosshair()
    {
        if (gunController == null)
            return;

        Vector3 targetPos =
            gunController.GetCrosshairWorldPosition();

        targetPos.z =
            transform.position.z;

        rb.MovePosition(
            Vector2.MoveTowards(
                rb.position,
                targetPos,
                moveSpeed *
                Time.fixedDeltaTime
            )
        );
    }


    // =========================================================
    // Enemy接触
    // =========================================================

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHP enemy =
            other.GetComponent<EnemyHP>();

        if (enemy == null)
            return;

        Die();
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        EnemyHP enemy =
            collision.gameObject.GetComponent<EnemyHP>();

        if (enemy == null)
            return;

        Die();
    }


    // =========================================================
    // 死亡
    // =========================================================

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (ResultManager.Instance != null)
        {
            ResultManager.Instance.ShowResult();
        }
    }


    // =========================================================
    // リザルトから復帰
    // =========================================================

    public void ResumeAfterResult()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = true;
        }

        isDead = false;

        skipNextMove = true;
    }
}