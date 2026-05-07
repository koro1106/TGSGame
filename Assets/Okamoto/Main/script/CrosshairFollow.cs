using UnityEngine;

public class CrosshairFollow : MonoBehaviour
{
    public RectTransform rectTransform;
    public Canvas canvas;

    [Range(0.1f, 10f)]
    public float sensitivity = 1f;

    private Vector2 currentPos;

    void Start()
    {
        currentPos = Input.mousePosition;
    }

    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        currentPos += new Vector2(mouseX, mouseY) * sensitivity * 20f;

        currentPos.x = Mathf.Clamp(currentPos.x, 0, Screen.width);
        currentPos.y = Mathf.Clamp(currentPos.y, 0, Screen.height);

        Vector2 pos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            currentPos,
            canvas.worldCamera,
            out pos
        );

        rectTransform.localPosition = pos;
    }

    public void SetSensitivity(float value)
    {
        sensitivity = value;

        Debug.Log("Š´“x: " + sensitivity);
    }
}