using UnityEngine;
/// <summary>
/// スキルツリードラッグ用
/// </summary>
public class UIDrag : MonoBehaviour
{
    public RectTransform target;     // スキルツリーの親オブジェクト
    private Vector2 lastMousePos;    // マウスがどのぐらい動いたかを出すための
    private bool isDragging = false; // ドラッグ中か

    [SerializeField] GameObject mainSkillTreeButon;
    [SerializeField] GameObject preSkillTreeButon;

    public bool isPrestige = false; // プレステージボタン押したか
    // プレステージ位置へ移動
    public void MoveToPrestige()
    {
        target.anchoredPosition = new Vector2(-16f, -3812f);
        isPrestige = true;
    }

    // 通常位置へ移動
    public void MoveToNormal()
    {
        target.anchoredPosition = new Vector2(26f, 8f);
        isPrestige = false;
    }
    void Update()
    {
        if (!isPrestige)
        {
            mainSkillTreeButon.SetActive(false);
            preSkillTreeButon.SetActive(true);

            // 押した瞬間
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                lastMousePos = Input.mousePosition;
            }

            // 離したら終了
            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            // ドラッグ中
            if (isDragging)
            {
                Vector2 currentMousePos = Input.mousePosition;
                Vector2 delta = currentMousePos - lastMousePos; // 差分

                // UI移動
                target.anchoredPosition += delta;

                lastMousePos = currentMousePos;
            }
        }
        else
        {
            mainSkillTreeButon.SetActive(true);
            preSkillTreeButon.SetActive(false);
        }
    }
}
