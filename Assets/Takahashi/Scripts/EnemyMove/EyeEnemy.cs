using UnityEngine;

public class EyeEnemy : MonoBehaviour
{
    public float moveSpeed = 4f;

    [Header("見た目")]
    public Transform graphics;

    [Header("ぴょんぴょん")]
    public float bounceHeight = 0.2f;
    public float bounceSpeed = 8f;

    [Header("頭パーツ(まばたき)")]
    public SpriteRenderer head;
    public Sprite headOpenSprite;   // 頭.png
    public Sprite headClosedSprite; // 頭_目閉じ.png
    public float blinkMinInterval = 2f; // 2?5秒でランダムに瞬き
    public float blinkMaxInterval = 5f;
    public float blinkDuration = 0.15f;

    [Header("手パーツ(Z回転で開閉)")]
    public Transform leftHand;
    public Transform rightHand;
    public float rightHandUpAngle = -25f;   // 右手: 上に行っている時
    public float rightHandDownAngle = 25f;  // 右手: 下に行っている時
    public float leftHandUpAngle = 25f;     // 左手: 上に行っている時(右手と逆)
    public float leftHandDownAngle = -25f;  // 左手: 下に行っている時(右手と逆)

    private Vector2 moveDirection;
    private float screenWidth;
    private float screenHeight;

    private EnemyHP hp;

    private float blinkTimer;
    private bool isBlinking;
    private float blinkTimeLeft;

    private SpriteRenderer bodySR;
    private SpriteRenderer tailSR;
    private SpriteRenderer headSR;
    private SpriteRenderer leftHandSR;
    private SpriteRenderer rightHandSR;

    void Start()
    {
        Camera cam = Camera.main;
        screenHeight = cam.orthographicSize;
        screenWidth = screenHeight * cam.aspect;

        hp = GetComponent<EnemyHP>();

        if (head != null) headSR = head;

        Transform body = transform.Find("身体");
        if (body != null) bodySR = body.GetComponent<SpriteRenderer>();

        Transform tail = transform.Find("しっぽ");
        if (tail != null) tailSR = tail.GetComponent<SpriteRenderer>();

        if (leftHand != null) leftHandSR = leftHand.GetComponent<SpriteRenderer>();
        if (rightHand != null) rightHandSR = rightHand.GetComponent<SpriteRenderer>();

        // ← 一番最後に呼ぶ
        SetSpawnAndDirection();

        if (head != null && headOpenSprite != null)
            head.sprite = headOpenSprite;

        ResetBlinkTimer();
    }

    void Update()
    {
        if (hp != null && hp.IsBind())
        {
            return;
        }
        if (hp == null) return;

        float hpRate = (float)hp.currentHP / hp.maxHP;

        // 移動 
        transform.Translate(
            moveDirection * moveSpeed * Time.deltaTime,
            Space.World
        );

        //  ぴょんぴょん 
        float currentBounce = bounceHeight * hpRate;
        Vector3 pos = transform.position;
        pos.y += Mathf.Sin(Time.time * bounceSpeed) * currentBounce;
        transform.position = pos;

        // まばたき 
        UpdateBlink();

        // 手の開閉(上昇中は内側、下降中は外側) 
        UpdateHandSway();
    }

    void ResetBlinkTimer()
    {
        blinkTimer = Random.Range(blinkMinInterval, blinkMaxInterval);
    }

    void UpdateBlink()
    {
        if (head == null) return;

        if (isBlinking)
        {
            blinkTimeLeft -= Time.deltaTime;
            if (blinkTimeLeft <= 0f)
            {
                isBlinking = false;
                head.sprite = headOpenSprite;
                ResetBlinkTimer();
            }
            return;
        }

        blinkTimer -= Time.deltaTime;
        if (blinkTimer <= 0f)
        {
            isBlinking = true;
            blinkTimeLeft = blinkDuration;
            head.sprite = headClosedSprite;
        }
    }

    void UpdateHandSway()
    {
        // Sin波の値(-1?1)をそのまま 0?1 に変換して使うことで、
        // 「上/下」の切り替え時に角度がカクッと変わらず連続的に動く。
        float wave = Mathf.Sin(Time.time * bounceSpeed);
        float t = (wave + 1f) * 0.5f; // 0(下) ? 1(上)

        if (rightHand != null)
        {
            float angle = Mathf.Lerp(rightHandDownAngle, rightHandUpAngle, t);
            rightHand.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        if (leftHand != null)
        {
            float angle = Mathf.Lerp(leftHandDownAngle, leftHandUpAngle, t);
            leftHand.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    void SetSpawnAndDirection()
    {
        Camera cam = Camera.main;
        float h = cam.orthographicSize;
        float w = h * cam.aspect;
        float halfW = w;
        float halfH = h;
        float offset = 3f; //（画面外距離）

        // 上からのスポーンは無し。右・左・下のみ。
        int side = Random.Range(0, 3);
        Vector2 spawnPos;

        switch (side)
        {
            case 0: // 右(高さは画面の半分の範囲)
                spawnPos = new Vector2(halfW + offset, Random.Range(-halfH / 2f, halfH / 2f));
                break;
            case 1: // 左(高さは画面の半分の範囲)
                spawnPos = new Vector2(-halfW - offset, Random.Range(-halfH / 2f, halfH / 2f));
                break;
            default: // 下
                spawnPos = new Vector2(Random.Range(-halfW, halfW), -halfH - offset);
                break;
        }

        transform.position = spawnPos;

        // 画面内ランダム地点へ向かう
        Vector2 target = new Vector2(
            Random.Range(-halfW, halfW),
            Random.Range(-halfH, halfH)
        );
        moveDirection = (target - spawnPos).normalized;

        bool flip = moveDirection.x > 0f;

        if (bodySR != null) bodySR.flipX = flip;
        if (tailSR != null) tailSR.flipX = flip;
        if (headSR != null) headSR.flipX = flip;
        if (leftHandSR != null) leftHandSR.flipX = flip;
        if (rightHandSR != null) rightHandSR.flipX = flip;
    }
}