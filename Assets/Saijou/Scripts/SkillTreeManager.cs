using UnityEngine;
/// <summary>
/// スキルツリー操作
/// </summary>
public class SkillTreeManager : MonoBehaviour
{
    public RectTransform target; // SkillTreeRoot（親）
    public float scaleSpeed = 0.1f; // 縮小・拡大速度
    public float minScale = 0.5f;   // 最少スケール
    public float maxScale = 2f;     // 最大スケール

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if(scroll != 0f)
        {
            float scale = target.localScale.x;
            scale += scroll * scaleSpeed;
            scale = Mathf.Clamp(scale, minScale, maxScale);

            target.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
