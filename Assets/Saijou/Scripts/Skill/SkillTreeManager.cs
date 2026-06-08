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

    public SkillData[] allSkills;
    public PlayerData playerData;
    void Start()
    {
        // セーブロード
        SaveManager.Load(playerData, allSkills);
        Debug.Log("LOAD");

        foreach (var skill in allSkills)
        {
            Debug.Log($"{skill.skillName} Lv:{skill.level} Unlock:{skill.isUnlocked}");
        }
       

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
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
