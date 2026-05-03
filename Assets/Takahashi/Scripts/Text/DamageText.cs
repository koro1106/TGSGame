using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    private TextMeshPro text;

    public float lifeTime = 1f;

    [Header("ジャンプ設定")]
    public float jumpForce = 600f;   // 上に飛ぶ力
    public float gravity = -150f;   // 落ちる力

    private float velocityY;

    void Awake()
    {
        text = GetComponent<TextMeshPro>();
    }

    public void SetDamage(int damage)
    {
        text.text = damage.ToString();

        // ダメージごとに色変更
        if (damage < 11)
        {
            text.color = Color.white; // 小ダメージ
        }
        else if (damage < 30)
        {
            text.color = Color.yellow; // 中ダメージ
        }
        else
        {
            text.color = Color.red; // 大ダメージ
        }
    }

    void Start()
    {
        // 上方向に初速を与える
        velocityY = jumpForce;
    }

    void Update()
    {
        // 重力で減速 → 落下
        velocityY += gravity * Time.deltaTime;

        // 上下移動
        transform.position += new Vector3(0, velocityY * Time.deltaTime, 0);

        // 時間で消える
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }
}