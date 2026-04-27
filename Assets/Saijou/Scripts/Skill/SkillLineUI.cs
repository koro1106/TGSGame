using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ラインUI表示用
/// </summary>
public class SkillLineUI : MonoBehaviour
{
    public Image image;
    public void SetState(SkillState state)
    {
        Color c = image.color;

        switch (state)
        {
            case SkillState.Locked:
                c.a = 0f;
                break;

            case SkillState.Available:
                c.a = 30f / 255f;
                break;

            case SkillState.Unlocked:
                c.a = 1f;
                break;
        }

        image.color = c;
    }
}
