using UnityEngine;

public class SkillTreeUI : MonoBehaviour
{
    [SerializeField] private SkillNodeUI[] nodes;

    /// <summary>
    /// スキルツリー全体の表示を更新
    /// </summary>
    public void RefreshAllNodes()
    {
        foreach (SkillNodeUI node in nodes)
        {
            if (node != null)
            {
                node.RefreshVisual();
            }
        }
    }
}
