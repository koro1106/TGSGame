using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    // =========================
    // 基本ステータス
    // =========================

    protected int maxHP = 40;      // 最大HP
    protected int currentHP;       // 現在HP

    // =========================
    // スケール関連
    // =========================

    protected Vector3 baseScale;   // 初期サイズ
    protected Vector3 targetScale; // 目標サイズ（HPに応じて変化）
    protected float scaleSmooth = 12f; // スケール変化の速さ

    // =========================
    // 初期化
    // =========================
    protected virtual void Start()
    {
        currentHP = maxHP;                 // HPを最大に初期化
        baseScale = transform.localScale;  // 初期サイズを保存
        targetScale = baseScale;           // 初期はそのままのサイズ
    }

    // =========================
    // 毎フレーム更新
    // =========================
    protected virtual void Update()
    {
        // HPに応じて変化するサイズへ滑らかに移動
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * scaleSmooth
        );
    }

    // =========================
    // ダメージ処理
    // =========================
    public void TakeDamage(int damage)
    {
        currentHP -= damage; // HPを減らす

        // HPが0以下なら死亡
        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            // HPに応じてスケール更新
            UpdateTargetScale();
        }
    }

    // =========================
    // スケール更新
    // =========================
    void UpdateTargetScale()
    {
        // HP割合（0〜1）
        float ratio = (float)currentHP / maxHP;

        // サイズも比例して小さくする
        targetScale = baseScale * ratio;
    }

    // =========================
    // 死亡処理
    // =========================
    void Die()
    {
        Destroy(gameObject); // オブジェクト削除
    }

    // =========================
    // レイヤー接触
    // =========================
    void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤーに触れたら即消滅
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}