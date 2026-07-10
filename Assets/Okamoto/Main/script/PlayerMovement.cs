using UnityEngine;
using UnityEngine.SceneManagement;

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

    public GunController gunController;

    private Rigidbody2D rb;
    private Camera cam;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    void FixedUpdate()
    {
        MoveToCrosshair();
    }

    // ========================
    // クロスヘアへ移動
    // ========================
    void MoveToCrosshair()
    {
        if (gunController == null)
            return;

        Vector3 screenPos = gunController.Crosshair.position;

        Vector3 targetPos = gunController.Cam.ScreenToWorldPoint(
            new Vector3(
                screenPos.x,
                screenPos.y,
                Mathf.Abs(gunController.Cam.transform.position.z)
            )
        );

        targetPos.z = transform.position.z;

        rb.MovePosition(
            Vector2.MoveTowards(
                rb.position,
                targetPos,
                moveSpeed * Time.fixedDeltaTime
            )
        );
    }

    // ========================
    // Enemy接触
    // ========================

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHP enemy = other.GetComponent<EnemyHP>();

        if (enemy == null)
            return;

        SceneManager.LoadScene(gameOverSceneName);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        EnemyHP enemy = collision.gameObject.GetComponent<EnemyHP>();

        if (enemy == null)
            return;

        SceneManager.LoadScene(gameOverSceneName);
    }
}