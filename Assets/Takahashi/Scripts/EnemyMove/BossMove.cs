using UnityEngine;
using System.Collections;

// ボスの「歩いて登場（プレイヤー追従なし）→狙いを定めてから直線に突進する」を管理するスクリプト。
//
// 使い方:
// 1. ボスのオブジェクトにこのスクリプトをアタッチ
// 2. walkDirection / walkDistance で「どっちに・どれだけ歩くか」を設定（うさぎと同じ、一人歩きするだけ）
//    ※もし既存のEnemyMoveなど専用の移動スクリプトを別で使う場合は、movementScriptToDisable にドラッグしてください
//      （その場合はこのスクリプトの内蔵の歩行は使わず、外部スクリプトに任せる形にも変更できます。教えてください）
// 3. telegraphPrefab に「狙いを示す赤いビーム」のPrefabアセットをドラッグ
//    → 実行時に自動でInstantiateされます。未設定でも動作しますが、その場合は見た目の予告なしでただ待つだけになります
// 4. playerLayer に、プレイヤーのレイヤーを設定
[RequireComponent(typeof(EnemyHP))]
public class BossMove : MonoBehaviour
{
    [Header("参照")]
    public Transform player;                   // プレイヤーのTransform（未設定ならタグ"Player"から自動取得）
    public Behaviour movementScriptToDisable;   // 突進中だけ止めたい外部の移動スクリプト（使っている場合のみ設定）

    // ★追加：ボス撃破をスポナーに通知するための参照
    // EnemySpawner.SpawnBoss() 側で自動的にセットされる
    public EnemySpawner spawner;

    [Header("移動（うさぎと同じ、一度だけ歩く。プレイヤー追従なし）")]
    public Vector2 walkDirection = Vector2.left; // 歩く方向（固定。プレイヤーは追わない）
    public float walkDistance = 5f;              // どれだけ歩いたら止まるか
    public float walkSpeed = 3f;                 // 歩く速度

    [Header("狙いを定める演出（画像の赤いビーム）")]
    public GameObject telegraphPrefab;          // 狙いを示す赤いビームのPrefab（ここにPrefabアセットをドラッグ）
    public float telegraphLength = 8f;          // ビームが伸びきったときの長さ
    public float aimDuration = 1f;              // 狙いを定めている時間（この間にプレイヤーは避ける準備ができる）

    private Transform telegraphVisual;          // ↑のPrefabを実際にInstantiateしたシーン上の実体（内部管理用）

    [Header("突進攻撃")]
    public float chargeSpeed = 18f;             // 突進スピード
    public float chargeDuration = 0.6f;         // 突進を続ける時間
    public int chargeDamage = 20;               // 突進が当たった時のダメージ
    public LayerMask playerLayer;               // 突進中の当たり判定用レイヤー
    public float chargeHitRadius = 0.6f;        // 突進中の当たり判定の半径

    [Header("タイミング")]
    public float appearDelay = 3f;              // 歩き終わってから最初の攻撃までの待ち時間
    public float attackInterval = 4f;           // 攻撃と攻撃の間のクールダウン

    [Header("デバッグ")]
    public KeyCode debugTriggerKey = KeyCode.T; // Playモード中にこのキーを押すと、待ち時間を無視して即座に突進攻撃を1回実行

    private EnemyHP hp;
    private bool isAttacking = false;           // 攻撃中の多重実行を防ぐフラグ

    void Awake()
    {
        hp = GetComponent<EnemyHP>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        //ボスのHPが0になって死亡演出が始まった瞬間、スポナーへ通知する
        hp.OnDeath += HandleBossDeath;

        // Prefabはシーン上に実体がないと表示できないので、ここで一度だけInstantiateしておく
        if (telegraphPrefab != null)
        {
            GameObject instance = Instantiate(telegraphPrefab, transform.position, Quaternion.identity);
            telegraphVisual = instance.transform;
            telegraphVisual.gameObject.SetActive(false);
        }
    }

    // ★追加：ボス撃破の通知処理。EnemyHPのOnDeathイベントから呼ばれる
    void HandleBossDeath()
    {
        if (spawner != null)
        {
            spawner.BossDefeated();
        }
    }

    void Start()
    {
        StartCoroutine(WalkInThenAttack());
    }

    void Update()
    {
        // デバッグ：指定キーを押すと、待ち時間やクールダウンを無視して即座に突進攻撃を1回試せる
        if (debugTriggerKey != KeyCode.None && Input.GetKeyDown(debugTriggerKey))
        {
            DebugTriggerChargeAttack();
        }
    }

    // インスペクターの ⋮ (右上の3点) または右クリックから「デバッグ：突進攻撃を今すぐ実行」で呼べます
    // ※Playモード中のみ動作します
    [ContextMenu("デバッグ：突進攻撃を今すぐ実行")]
    public void DebugTriggerChargeAttack()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("BossMove: Playモード中のみデバッグ実行できます。");
            return;
        }

        if (isAttacking)
        {
            Debug.Log("BossMove: 現在攻撃中のため、デバッグ実行をスキップしました。");
            return;
        }

        if (player == null)
        {
            Debug.LogWarning("BossMove: playerが未設定のため、デバッグ実行できません。");
            return;
        }

        StartCoroutine(DoChargeAttack());
    }

    // ===== 歩いて登場 → 攻撃ループ =====
    IEnumerator WalkInThenAttack()
    {
        // 外部の移動スクリプトを使う場合は、こちらの内蔵歩行はスキップする
        if (movementScriptToDisable == null)
        {
            yield return StartCoroutine(WalkIn());
        }

        yield return StartCoroutine(AttackLoop());
    }

    // うさぎと同じ「一人歩き」：決まった方向に決まった距離だけ歩いて止まる（プレイヤーは追わない）
    IEnumerator WalkIn()
    {
        Vector3 start = transform.position;
        Vector3 target = start + (Vector3)(walkDirection.normalized * walkDistance);

        while (Vector2.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, walkSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;
    }

    // 一定間隔で突進攻撃を繰り返す
    IEnumerator AttackLoop()
    {
        // 歩き終わってすぐは攻撃しない
        // （画面外を歩いている途中で、画面外から攻撃されてプレイヤーが避けられない…という事故を防ぐ）
        yield return new WaitForSeconds(appearDelay);

        while (true)
        {
            // ボスが死んでいたらループを止める
            if (hp != null && hp.currentHP <= 0)
            {
                yield break;
            }

            yield return StartCoroutine(DoChargeAttack());

            yield return new WaitForSeconds(attackInterval);
        }
    }

    IEnumerator DoChargeAttack()
    {
        if (player == null) yield break;

        isAttacking = true;

        // 外部の移動スクリプトを一時停止（歩行AIと突進が同時に動かないように）
        if (movementScriptToDisable != null)
        {
            movementScriptToDisable.enabled = false;
        }

        // ===== 狙いを定める演出 =====
        Vector3 origin = transform.position; // 狙いを定めた瞬間のボス位置（ビームの根本＝突進の起点）
        Vector2 aimDir = ((Vector2)player.position - (Vector2)origin).normalized;

        if (telegraphVisual != null)
        {
            telegraphVisual.gameObject.SetActive(true);

            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            telegraphVisual.rotation = Quaternion.Euler(0f, 0f, angle);

            // ビームを0から目標の長さまでだんだん伸ばす。
            // ※SpriteのPivotが中央の場合、scaleだけ伸ばすと前後に均等に伸びてしまうため、
            //   ボス位置(origin)を起点に、伸びた分の半分だけ狙った方向へpositionもずらしている。
            //   （これでボスの位置から狙った方向へ真っ直ぐ伸びていくように見える）
            Vector3 baseScale = telegraphVisual.localScale;
            float t = 0f;

            while (t < aimDuration)
            {
                t += Time.deltaTime;
                float ratio = Mathf.Clamp01(t / aimDuration);
                float currentLength = telegraphLength * ratio;

                telegraphVisual.localScale = new Vector3(
                    currentLength,
                    baseScale.y,
                    baseScale.z
                );

                telegraphVisual.position = origin + (Vector3)(aimDir * currentLength * 0.5f);

                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(aimDuration);
        }

        if (telegraphVisual != null)
        {
            telegraphVisual.gameObject.SetActive(false);
        }

        // ===== 突進（攻撃） =====
        // 狙いを定めた瞬間の方向に固定して突進する（突進中はプレイヤーを追いかけ直さない）
        Vector2 chargeDir = aimDir;
        float chargeTimer = 0f;

        while (chargeTimer < chargeDuration)
        {
            chargeTimer += Time.deltaTime;

            Vector2 move = chargeDir * chargeSpeed * Time.deltaTime;
            transform.position += (Vector3)move;

            // 突進中にプレイヤーへ当たったかチェック
            Collider2D hitPlayer = Physics2D.OverlapCircle(transform.position, chargeHitRadius, playerLayer);
            if (hitPlayer != null)
            {
                // プレイヤー側のスクリプトに TakeDamage(int) があれば呼ばれる
                // （関数名が違う場合はここを合わせて変更してください）
                hitPlayer.SendMessage("TakeDamage", chargeDamage, SendMessageOptions.DontRequireReceiver);
                break; // 1回の突進で複数ヒットしないように抜ける
            }

            yield return null;
        }

        // 外部の移動スクリプトを再開
        if (movementScriptToDisable != null)
        {
            movementScriptToDisable.enabled = true;
        }

        isAttacking = false;
    }

    // デバッグ用：シーンビューで当たり判定範囲を確認できるように
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chargeHitRadius);
    }
}