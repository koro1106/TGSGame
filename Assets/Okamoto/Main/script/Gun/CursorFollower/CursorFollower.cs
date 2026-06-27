using UnityEngine;

public class CursorFollower : MonoBehaviour
{
    public RectTransform crosshair;

    public Transform gunPivot;
    public Transform gunImage;

    private Camera cam;

    private Vector3 defaultLocalPos;

    void Start()
    {
        cam = Camera.main;

        defaultLocalPos = gunImage.localPosition;
    }

    void Update()
    {
        Vector3 screenPos = crosshair.position;

        Vector3 worldPos =
            cam.ScreenToWorldPoint(screenPos);

        worldPos.z = 0;

        Vector3 dir =
            worldPos - gunPivot.position;

        float angle =
            Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        bool isLeft = dir.x < 0;

        // ‰ñ“]
        gunPivot.rotation =
            Quaternion.Euler(0, 0, angle);

        // ¶‰E”½“]
        if (isLeft)
        {
            gunImage.localScale =
                new Vector3(1, -1, 1);
        }
        else
        {
            gunImage.localScale =
                new Vector3(1, 1, 1);
        }

        // ˆÊ’uŒÅ’è
        gunImage.localPosition =
            defaultLocalPos;
    }
}