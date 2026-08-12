using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ラインUI表示用
/// </summary>
public class SkillLineUI : MonoBehaviour
{
    public Image image;

    [Header("点線アニメーション")]
    [SerializeField] private float scrollSpeed = 0.15f;

    private Material lineMaterial;

    private void Awake()
    {
        // 他のラインとMaterial共有しないように複製
        lineMaterial = Instantiate(image.material);
        image.material = lineMaterial;
    }

    void Update()
    {
        if (lineMaterial == null)
            return;

        Vector2 offset = lineMaterial.mainTextureOffset;

        // 点線を上方向へ流す
        offset.y += scrollSpeed * Time.deltaTime;

        lineMaterial.mainTextureOffset = offset;
    }

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
