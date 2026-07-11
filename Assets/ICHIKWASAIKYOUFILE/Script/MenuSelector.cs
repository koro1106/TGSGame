using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSelector : MonoBehaviour
{
    [SerializeField] private RectTransform arrow;
    [SerializeField] private RectTransform[] menuItems;
    [SerializeField] private RectTransform[] arrowPoints;

    [SerializeField] private RectTransform menuPanel;
    [SerializeField] private OptionMenu optionMenu;

    private int index = 0;
    private bool canSelect = true;

    private void Start()
    {
        MoveArrow();
    }

    private void Update()
    {
        if (!canSelect)
            return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Select();
        }
    }

    public void Select()
    {
        switch (index)
        {
            case 0:
                Debug.Log("ゲーム開始");
                SceneManager.LoadScene("MainStageScene");
                break;

            case 1:
                Debug.Log("オプション");
                optionMenu.OpenOption();
                break;

            case 2:
                Debug.Log("ゲーム終了");
                Application.Quit();
                break;

            case 3:
                optionMenu.CloseOption();
                break;
        }
    }


    public void SetIndex(int newIndex)
    {
        Debug.Log("SetIndex呼び出し Index=" + newIndex);

        if (newIndex >= menuItems.Length)
            newIndex = 0;

        if (newIndex < 0)
            newIndex = menuItems.Length - 1;

        index = newIndex;
        MoveArrow();
    }


    private void MoveArrow()
    {
        RectTransform point = arrowPoints[index];

        Vector3 worldPos = point.position;

        arrow.SetPositionAndRotation(
            worldPos,
            arrow.rotation
        );
    }


    private bool IsInsideCanvas(RectTransform rect)
    {
        Canvas canvas = rect.GetComponentInParent<Canvas>();

        if (canvas == null)
            return true;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        Vector3[] objectCorners = new Vector3[4];
        Vector3[] canvasCorners = new Vector3[4];

        rect.GetWorldCorners(objectCorners);
        canvasRect.GetWorldCorners(canvasCorners);


        // 横方向だけ確認
        bool insideX =
            objectCorners[2].x > canvasCorners[0].x &&
            objectCorners[0].x < canvasCorners[2].x;

        // 縦方向も確認
        bool insideY =
            objectCorners[2].y > canvasCorners[0].y &&
            objectCorners[0].y < canvasCorners[2].y;


        return insideX && insideY;
    }
}