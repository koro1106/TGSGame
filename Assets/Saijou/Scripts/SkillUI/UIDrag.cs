using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// スキルツリードラッグ用
/// </summary>
public class UIDrag : MonoBehaviour
{
    public RectTransform target;     // スキルツリーの親オブジェクト
    private Vector2 lastMousePos;    // マウスがどのぐらい動いたかを出すための
    private bool isDragging = false; // ドラッグ中か

    [SerializeField] GameObject skillTreeButon;
    [SerializeField] GameObject prestigeButon;
    [SerializeField] GameObject shopButon;

    [SerializeField] Image skillTreeImage;
    [SerializeField] Image prestigeImage;
    [SerializeField] Image shopImage;

    [SerializeField] private Outline skillTreeOutline;
    [SerializeField] private Outline prestigeOutline;
    [SerializeField] private Outline shopOutline;

    public bool isPrestige = true; 

    [SerializeField] SkillTreeChange skillTreeChange;
    // プレステージ位置へ移動
    public void MoveToPrestige()
    {
        target.anchoredPosition = new Vector2(-16f, -4100f);
        isPrestige = false;
        UpdateButtonAlpha();
    }

    // 通常位置へ移動
    public void MoveToNormal()
    {
        target.anchoredPosition = new Vector2(26f, 8f);
        isPrestige = true;
        UpdateButtonAlpha();
    }

    void Start()
    {
        skillTreeImage = skillTreeButon.GetComponent<Image>();
        prestigeImage = prestigeButon.GetComponent<Image>();
        shopImage = shopButon.GetComponent<Image>();

        UpdateButtonAlpha();
    }

    void Update()
    {
        if (isPrestige)
        {
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

                // 一旦移動
                Vector2 newPos = target.anchoredPosition + delta;

                // 範囲制限
                newPos.x = Mathf.Clamp(newPos.x, -700f, 700f);
                newPos.y = Mathf.Clamp(newPos.y, -700f, 500f);

                // UI移動
                target.anchoredPosition = newPos;

                lastMousePos = currentMousePos;
            }
        }
    }

    public void UpdateButtonAlpha()
    {
        if (isPrestige)
        {
            skillTreeImage.color = new Color32(100, 100, 100, 255);
            prestigeImage.color = new Color32(255, 255, 255, 255);
            shopImage.color = new Color32(100, 100, 100, 255);

            // 枠
            //skillTreeOutline.enabled = false;
            //prestigeOutline.enabled = true;
            //shopOutline.enabled = false;
        }
        else
        {
            skillTreeImage.color = new Color32(255, 255, 255, 255);
            prestigeImage.color = new Color32(100, 100, 100, 255);
            shopImage.color = new Color32(100, 100, 100, 255);

            // 枠
            //skillTreeOutline.enabled = true;
            //prestigeOutline.enabled = false;
            //shopOutline.enabled = false;
        }
        if(skillTreeChange.isShop)
        {
            skillTreeImage.color = new Color32(100, 100, 100, 255);
            prestigeImage.color = new Color32(100, 100, 100, 255);
            shopImage.color = new Color32(255, 255, 255, 255);

            // 枠
        //    skillTreeOutline.enabled = false;
        //    prestigeOutline.enabled = false;
        //    shopOutline.enabled = true;
        }
    }
}
