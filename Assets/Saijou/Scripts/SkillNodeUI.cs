using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// スキルの状態
/// </summary>
public enum SkillState
{
    Locked,     // 未解放（完全に見えない）
    Available,  // 解放可能（うっすら表示）
    Unlocked    // 解放済み（完全表示）
}

/// <summary>
/// スキルツリーノードUI管理
/// </summary>
public class SkillNodeUI : MonoBehaviour
{
    public SkillData data;     // 紐づくスキルデータ
    public Image icon;         // ボタン画像

    public SkillNodeUI[] nextButtons; // 次に解放されるスキルボタン
    public SkillLineUI[] nextLines;   // 次に解放されるスキルライン

    [SerializeField] GameObject unlockEffectPrefab; // 解放エフェクト

    private SkillState state = SkillState.Locked; // 現在の状態（初期は未解放）
    public bool isStartNode = false; // 最初から表示するノード
    void Start()
    {
        // 最初のノードなら「解放可能状態」にする
        if (isStartNode)
        {
            state = SkillState.Available;
        }


        if (data.isUnlocked)
        {
            state = SkillState.Unlocked;
        }
        else if (isStartNode)
        {
            state = SkillState.Available;
        }
        else
        {
            state = SkillState.Locked;
        }

        // ラインも状態に応じて復元
        foreach (var line in nextLines)
        {
            if (state == SkillState.Unlocked)
                line.SetState(SkillState.Available);
            else
                line.SetState(SkillState.Locked);
        }

        UpdateVisual();
    }

   // スキル解放
   public void Unlock()
   {
        // すでに解放済みなら何もしない
        if (state == SkillState.Unlocked) return;

        // 状態を解放済みに変更
        state = SkillState.Unlocked;
        data.isUnlocked = true;

        UpdateVisual();

        // 次のスキルを「解放可能状態」にする（透明度30）
       foreach (var node in nextButtons) // ボタン
       {
           node.SetButtonAvailable();
           SpawnEffect(node.transform); // パーティクル生成
       }
       foreach (var line in nextLines) // ライン
       {
           line.SetState(SkillState.Available);
       }
   }

    // スキルを「解放可能(うっすら表示)」状態にする(ボタン)
    public void SetButtonAvailable()
    {
        // すでに解放済み or すでに表示中なら何もしない
        if (state != SkillState.Locked) return;

        state = SkillState.Available;
        UpdateVisual();
    }

    // 状況に応じて見た目更新
    void UpdateVisual()
    {
        Color c = icon.color;

        if (data.isUnlocked)
        {
            state = SkillState.Unlocked; // 解放済みに
        }
        switch (state)
        {
            case SkillState.Locked:
                c.a = 0f; // 完全非表示
                break;

            case SkillState.Available:
                c.a = 30f / 255f; // うっすら表示
                break;
        
            case SkillState.Unlocked:
                c.a = 1f; // 完全表示
                break;
        }

        icon.color = c;
    }

    // Click処理
    public void OnClick()
    {
        if (data.currentExp >= data.needExp)
        {
            data.TryLevelUp(); // レベルアップ
            Unlock();
            UpdateVisual();
        }
    }

    void SpawnEffect(Transform target)
    {
        if (unlockEffectPrefab == null) return;

        RectTransform targetRt = target.GetComponent<RectTransform>();
        Canvas canvas = GetComponentInParent<Canvas>();

        Vector3 worldPos;

        // Canvasの種類で処理変える
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // カメラなしなのでそのままスクリーン→ワールド変換
            worldPos = Camera.main.ScreenToWorldPoint(targetRt.position);
        }
        else
        {
            // Camera or WorldSpace
            worldPos = Camera.main.ScreenToWorldPoint(
                RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, targetRt.position)
            );
        }

        // ★ Zをちゃんと前に出す（重要）
        worldPos.z = 0f;

        Instantiate(unlockEffectPrefab, worldPos, Quaternion.identity);
    }
}
