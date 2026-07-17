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

    public void OnPointerEnterItem(int newIndex)
    {
        Debug.Log("Pointer Enter : " + newIndex);
       
        SetIndex(newIndex);
    }

    public void Test()
    {
        Debug.Log("呼ばれた！");
    }
    
}