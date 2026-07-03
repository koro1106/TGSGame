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

    [SerializeField] Sprite redSprite;     // 経験値不足：赤
    [SerializeField] Sprite greenSprite;   // 解放済み：緑
    [SerializeField] Sprite yellowSprite;  // 最大レベル：黄

    [SerializeField] SkillEffectManager effectManager;
    [SerializeField] NormalExpText normalExpText;
    [SerializeField] ExpUIAnimation expUIAnimation;
    [SerializeField] PrestigeExpText prestigelExpText;
    [SerializeField] PrestigeExpUIAnimation prestigeExpUIAnimation;
    [SerializeField] UIAnimation uiAnimation;
    [SerializeField] PlayerStats playerStats;

    public PlayerData playerData;
    public SkillData[] allSkills;

    void Start()
    {
        // 最初のノードなら「解放可能状態」にする
        if (isStartNode)
        {
            state = SkillState.Available;
        }

        // ラインも状態に応じて復元
        foreach (var line in nextLines)
        {
            if (state == SkillState.Unlocked)
                line.SetState(SkillState.Available);
            else
                line.SetState(SkillState.Locked);
        }

        //UpdateVisual();
        RestoreState();
    }

    public void RestoreState()
    {
        // このスキルが解放済みの場合
        if (data.isUnlocked)
        {
            // 自身を解放済み状態にする
            state = SkillState.Unlocked;

            // 次のスキルを解放可能状態にする
            foreach (var node in nextButtons)
            {
                if (!node.data.isUnlocked)
                    node.SetButtonAvailable();
            }

            // 次のラインも表示状態にする
            foreach (var line in nextLines)
            {
                line.SetState(SkillState.Available);
            }
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
           line.SetState(SkillState.Unlocked);
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

        bool hasExp = GetCurrentExp() >= data.needExp;
        bool isMax = data.IsMaxLevel(); // MAXレベルかどうか

        if (data.isUnlocked)
        {
            state = SkillState.Unlocked; // 解放済みに
        }
        switch (state)
        {
            case SkillState.Locked:// 完全非表示
                icon.enabled = false; 
                break;

            case SkillState.Available:
                icon.enabled = true; // 解放可能状態

                icon.sprite = greenSprite;

                if (hasExp)
                {
                    // 経験値足りてる → 薄緑
                    c.a = 100f / 255f;
                }
                else
                {
                    // 経験値足りない → 赤画像にする
                    icon.sprite = redSprite;
                    c.a = 100f / 255f;
                }
                break;

            case SkillState.Unlocked:
                icon.enabled = true;
                c.a = 1f; // 完全表示
                if (isMax)
                {
                    // 最大レベル → 黄色
                    icon.sprite = yellowSprite;
                }
                else
                {
                    // 通常解放 → 緑
                    icon.sprite = greenSprite;
                }
                break;
        }

        icon.color = c;
    }

    // Click処理
    public void OnClick()
    {
        // 最大レベルなら何もしない
        if (data.IsMaxLevel()) return;

        // 経験値足りないなら何もしない
        if (GetCurrentExp() < data.needExp) return;
        
        data.TryLevelUp();             // レベルアップ
        effectManager.ApplySkill(data);// スキル効果適用
        SEManager.Instance.PlayLevelUpSE(); // SE再生
        Unlock();                      // 解放
        UpdateVisual();                // 見た目更新

        SaveManager.Save(playerData, allSkills); // セーブ

        // 経験値UIアップデート
        if (normalExpText != null)
        {
            normalExpText.UpdateNormalExpText();
        }

        if (prestigelExpText != null)
        {
            prestigelExpText.UpdatePrestigeExpText();
        }

        PlayExpAnimation(); // 経験値UIアニメーション
    }

    // 経験値UIアニメーション
    public void PlayExpAnimation()
    {
        switch (data.expType)
        {
            case ExpType.Exp1:
                uiAnimation.PlayBounce(expUIAnimation.exp_1.rectTransform);
                break;

            case ExpType.Exp2:
                uiAnimation.PlayBounce(expUIAnimation.exp_2.rectTransform);
                break;

            case ExpType.Exp3:
                uiAnimation.PlayBounce(expUIAnimation.exp_3.rectTransform);
                break;

            case ExpType.PreExp:
                uiAnimation.PlayBounce(expUIAnimation.preExp.rectTransform);
                break;
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

        // Zを前に出す
        worldPos.z = 0f;

        Instantiate(unlockEffectPrefab, worldPos, Quaternion.identity);
    }


    // 経験値取得関数
    int GetCurrentExp()
    {
        switch (data.expType)
        {
            case ExpType.Exp1:
                return data.playerData.currentExp_1;

            case ExpType.Exp2:
                return data.playerData.currentExp_2;

            case ExpType.Exp3:
                return data.playerData.currentExp_3;

            case ExpType.PreExp:
                return data.playerData.currentPreExp;
        }

        return 0;
    }
}
