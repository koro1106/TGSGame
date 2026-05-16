using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    private TextMeshPro text;

    [Header("表示時間")]
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

        // 通常色
        text.color = Color.white;
        text.fontSize = 100;
        text.fontStyle = FontStyles.Normal;
        /* ダメージごとに色変更
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
        }*/
    }

    //クリティカル表示
    public void SetCritical()
    {
        //色変更
        text.color = new Color(1f, 0.5f, 0f);

        //サイズ大きく
        text.fontSize *= 1.5f;

        //太字
        text.fontStyle = FontStyles.Bold;
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