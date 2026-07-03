using UnityEngine;

public class MenuSelector : MonoBehaviour
{
    [SerializeField] private RectTransform arrow;
    [SerializeField] private RectTransform[] menuItems;

    [SerializeField] private RectTransform[] arrowPoints;


    private int index = 0;

    private void Start()
    {
        MoveArrow();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SetIndex(index + 1);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SetIndex(index - 1);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("選択：" + menuItems[index].name);

            switch (index)
            {
                case 0:
                    Debug.Log("ゲーム開始");
                    break;

                case 1:
                    Debug.Log("オプション");
                    break;

                case 2:
                    Debug.Log("ゲーム終了");
                    Application.Quit();
                    break;
            }
        }
    }

    public void SetIndex(int newIndex)
    {
        if (newIndex >= menuItems.Length)
            newIndex = 0;

        if (newIndex < 0)
            newIndex = menuItems.Length - 1;

        index = newIndex;
        MoveArrow();
    }

    private void MoveArrow()
    {
        arrow.position = arrowPoints[index].position;
    }
}