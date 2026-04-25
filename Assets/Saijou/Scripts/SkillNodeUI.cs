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

    public SkillNodeUI[] nextNodes; // 次に解放されるスキル

    private SkillState state = SkillState.Locked; // 現在の状態（初期は未解放）
    public bool isStartNode = false; // 最初から表示するノード
    void Start()
    {
        // 最初のノードなら「解放可能状態」にする
        if (isStartNode)
        {
            state = SkillState.Available;
        }

        // 初期状態を反映
        UpdateVisual();
    }

    // スキル解放
   public void Unlock()
   {
        // すでに解放済みなら何もしない
        if (state == SkillState.Unlocked) return;

        // 状態を解放済みに変更
        state = SkillState.Unlocked;
        UpdateVisual();

        // 次のスキルを「解放可能状態」にする（透明度30）
       foreach (var node in nextNodes)
       {
           node.SetAvailable();
       }
   }

    // スキルを「解放可能(うっすら表示)」状態にする
    public void SetAvailable()
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
}
