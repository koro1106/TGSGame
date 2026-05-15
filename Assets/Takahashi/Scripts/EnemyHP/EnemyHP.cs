using UnityEngine;
using System.Collections;

public class EnemyHP : MonoBehaviour
{
    [Header("HP")]
    public int maxHP = 100;      // 最大HP
    public int currentHP;        // 現在HP

    [Header("見た目")]
    public float scaleSmooth = 12f; // スケール補間速度

    [Header("時間経過HP増加")]
    public float growInterval = 10f;    // HP増加間隔
    public float growMultiplier = 1.5f; // HP増加倍率

    private float growTimer = 0f; // 経過時間

    [Header("死亡演出")]
    public float deathDuration = 1.2f;   // 死亡演出時間
    public float spiralSpeed = 20f;      // 渦スピード
    public float rotationSpeed = 1080f;  // 回転速度

    [Header("ドロップ")]
    public GameObject dropPrefab; // ドロップアイテム

    [Header("ダメージ表示")]
    public GameObject damageText; // ダメージUI

    private Vector3 baseScale;   // 初期スケール
    private Vector3 targetScale; // 目標スケール

    private bool isDying = false; // 死亡中フラグ
    private Collider2D col;       // コライダー

    void Start()
    {
        // HP初期化
        currentHP = maxHP;

        // スケール保存
        baseScale = transform.localScale;
        targetScale = baseScale;

        // コライダー取得
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        // 死亡中は処理しない
        if (isDying) return;

        // スケール反映

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * scaleSmooth
        );
    }

    // ダメージ処理
    public void TakeDamage(int damage)
    {
        if (isDying) return;

        currentHP -= damage;              // HP減少
        currentHP = Mathf.Max(currentHP, 0); // 0以下防止

        ShowDamage(damage); // ダメージ表示
        UpdateScale();      // 見た目更新

        if (currentHP <= 0)
        {
            StartCoroutine(DeathSpiral()); // 死亡処理
        }
    }

    // HP割合でサイズ変更
    void UpdateScale()
    {
        float ratio = 1f;

        if (currentHP > maxHP * 0.75f)
            ratio = 1f;
        else if (currentHP > maxHP * 0.5f)
            ratio = 0.875f;
        else if (currentHP > maxHP * 0.25f)
            ratio = 0.75f;
        else
            ratio = 0.625f;

        targetScale = baseScale * ratio;
    }

    // HP増加処理
    void GrowHP()
    {
        maxHP = Mathf.RoundToInt(maxHP * growMultiplier);     // 最大HP増加
        currentHP = Mathf.RoundToInt(currentHP * growMultiplier); // 現在HP増加

        currentHP = Mathf.Min(currentHP, maxHP); // 上限制御

        UpdateScale(); // 見た目更新
    }

    // 死亡演出
    IEnumerator DeathSpiral()
    {
        isDying = true;

        // 当たり判定OFF
        if (col != null)
            col.enabled = false;

        // ドロップ生成
        if (dropPrefab != null)
            Instantiate(dropPrefab, transform.position, Quaternion.identity);

        Vector3 startScale = transform.localScale;
        float timer = 0f;

        while (timer < deathDuration)
        {
            timer += Time.deltaTime;
            float t = timer / deathDuration;

            // 回転
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

            // 渦巻き移動
            float radius = Mathf.Lerp(0.5f, 0f, t);

            Vector3 spiral = new Vector3(
                Mathf.Cos(timer * spiralSpeed),
                Mathf.Sin(timer * spiralSpeed),
                0
            ) * radius;

            transform.position += spiral * Time.deltaTime;

            // 縮小
            float scale = Mathf.Lerp(1f, 0f, t);
            transform.localScale = startScale * (scale * scale);

            yield return null;
        }

        Destroy(gameObject);
    }

    // ダメージUI表示
    void ShowDamage(int damage)
    {
        if (damageText == null) return;

        GameObject obj = Instantiate(
            damageText,
            transform.position,
            Quaternion.identity
        );

        DamageText dmg = obj.GetComponent<DamageText>();

        if (dmg != null)
            dmg.SetDamage(damage);
    }
}