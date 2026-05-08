using UnityEngine;
using System.Collections;

public class EnemyHP : MonoBehaviour
{
    [Header("HP")]
    public int maxHP = 100;
    public int currentHP;

    [Header("見た目")]
    public float scaleSmooth = 12f;

    [Header("死亡演出")]
    public float deathDuration = 1.2f;
    public float spiralSpeed = 20f;
    public float rotationSpeed = 1080f;

    [Header("ドロップ")]
    public GameObject dropPrefab;

    [Header("ダメージ表示")]
    public GameObject damageText;

    private Vector3 baseScale;
    private Vector3 targetScale;

    private bool isDying = false;

    void Start()
    {
        currentHP = maxHP;

        baseScale = transform.localScale;
        targetScale = baseScale;
    }

    void Update()
    {
        // 通常時だけサイズ変更
        if (!isDying)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                targetScale,
                Time.deltaTime * scaleSmooth
            );
        }
    }

    // ダメージ処理
    public void TakeDamage(int damage)
    {
        // 死亡演出中は無効
        if (isDying) return;

        currentHP -= damage;

        // 0以下にならないように
        currentHP = Mathf.Max(currentHP, 0);

        // ダメージ表示
        ShowDamage(damage);

        // サイズ更新
        UpdateScale();

        // HP0で死亡
        if (currentHP <= 0)
        {
            StartCoroutine(DeathSpiral());
        }
    }

    // HP段階でサイズ変更
    void UpdateScale()
    {
        float ratio = 1f;

        // 40〜31
        if (currentHP > maxHP * 3 / 4)
        {
            ratio = 8f / 8f;
        }
        // 30〜21
        else if (currentHP > maxHP * 2 / 4)
        {
            ratio = 7f / 8f;
        }
        // 20〜11
        else if (currentHP > maxHP * 1 / 4)
        {
            ratio = 6f / 8f;
        }
        // 10〜0
        else
        {
            ratio = 5f / 8f;
        }

        targetScale = baseScale * ratio;
    }

    // 渦巻きながら消滅
    IEnumerator DeathSpiral()
    {
        isDying = true;

        // ドロップ生成
        if (dropPrefab != null)
        {
            Instantiate(dropPrefab, transform.position, Quaternion.identity);
        }

        Vector3 startScale = transform.localScale;

        float timer = 0f;

        while (timer < deathDuration)
        {
            timer += Time.deltaTime;

            float t = timer / deathDuration;

            // 回転
            float rot = Mathf.Lerp(
                rotationSpeed,
                rotationSpeed * 2f,
                t
            );

            transform.Rotate(0, 0, rot * Time.deltaTime);

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

            // 最後に吸い込まれる感じ
            scale *= scale;

            transform.localScale = startScale * scale;

            yield return null;
        }

        Destroy(gameObject);
    }

    // ダメージ表示
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
        {
            dmg.SetDamage(damage);
        }
    }
}