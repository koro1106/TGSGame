using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("移動するシーン名")]
    public string gameOverSceneName;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // WASD入力
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // 斜め移動速度を統一
        moveInput = moveInput.normalized;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    // ========================
    // Enemy接触
    // ========================

    void OnTriggerEnter2D(Collider2D other)
    {
        // EnemyHP取得
        EnemyHP enemy =
            other.GetComponent<EnemyHP>();

        // EnemyHPが無ければ無視
        if (enemy == null)
            return;

        // シーン移動
        SceneManager.LoadScene(gameOverSceneName);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // EnemyHP取得
        EnemyHP enemy =
            collision.gameObject.GetComponent<EnemyHP>();

        // EnemyHPが無ければ無視
        if (enemy == null)
            return;

        // シーン移動
        SceneManager.LoadScene(gameOverSceneName);
    }
}