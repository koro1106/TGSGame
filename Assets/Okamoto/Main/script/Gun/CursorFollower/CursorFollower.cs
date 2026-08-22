using UnityEngine;

public class CursorFollower : MonoBehaviour
{
    [Header("照準")]
    public RectTransform crosshair;

    [Header("クロスヘアがあるCanvas")]
    public Canvas canvas;

    [Header("銃画像")]
    public Transform gunImage;

    private Camera cam;
    private Vector3 defaultScale;

    void Start()
    {
        cam = Camera.main;

        // 最初のScaleを保存
        defaultScale = gunImage.localScale;
    }

    void Update()
    {
        Aim();
    }

    void Aim()
    {
        // =================================
        // クロスヘアのワールド座標を取得
        // =================================
        Vector3 crosshairWorldPos;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // Overlayの場合
            Vector2 screenPos =
                RectTransformUtility.WorldToScreenPoint(
                    null,
                    crosshair.position
                );

            crosshairWorldPos =
                cam.ScreenToWorldPoint(
                    new Vector3(
                        screenPos.x,
                        screenPos.y,
                        Mathf.Abs(
                            cam.transform.position.z
                            - gunImage.position.z
                        )
                    )
                );
        }
        else
        {
            // Screen Space Camera / World Spaceの場合
            crosshairWorldPos =
                crosshair.position;
        }

        crosshairWorldPos.z = gunImage.position.z;

        // =================================
        // 銃のPivot → クロスヘアの方向
        // =================================
        Vector2 direction =
            crosshairWorldPos - gunImage.position;

        // =================================
        // 360°の角度を取得
        // =================================
        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        // =================================
        // クロスヘア方向へ回転
        // =================================
        gunImage.rotation =
    Quaternion.Euler(0f, 0f, angle + 180f);

        // =================================
        // 左右反転
        // =================================
        if (direction.x < 0)
        {
            // 左側
            gunImage.localScale =
                new Vector3(
                    defaultScale.x,
                    defaultScale.y,
                    defaultScale.z
                );
        }
        else
        {
            // 右側
            gunImage.localScale =
                new Vector3(
                    defaultScale.x,
                    -defaultScale.y,
                    defaultScale.z
                );
        }
    }
}