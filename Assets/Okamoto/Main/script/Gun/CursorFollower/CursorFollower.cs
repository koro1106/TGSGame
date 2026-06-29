using UnityEngine;

public class CursorFollower : MonoBehaviour
{
    [Header("Æ€")]
    public RectTransform crosshair;

    [Header("‰ñ“]‚Ì’†S(Empty)")]
    public Transform gunCenter;

    [Header("e‰æ‘œ")]
    public Transform gunImage;

    private Camera cam;
    private Vector3 defaultLocalPos;

    void Start()
    {
        cam = Camera.main;

        // e‰æ‘œ‚Ì‰ŠúˆÊ’u‚ð•Û‘¶
        defaultLocalPos = gunImage.localPosition;
    }

    void Update()
    {
        Aim();
    }

    void Aim()
    {
        // ƒNƒƒXƒwƒA‚ðƒ[ƒ‹ƒhÀ•W‚Ö•ÏŠ·
        Vector3 worldPos = cam.ScreenToWorldPoint(crosshair.position);
        worldPos.z = 0f;

        // GunCenter‚©‚çƒNƒƒXƒwƒA‚Ö‚Ì•ûŒü
        Vector3 dir = worldPos - gunCenter.position;

        // ‰ñ“]Šp“x
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // GunCenter‚¾‚¯‰ñ“]
        gunCenter.rotation = Quaternion.Euler(0f, 0f, angle);

        // ¶‰E”½“]
        if (angle > 90f || angle < -90f)
        {
            gunImage.localScale = new Vector3(1f, -1f, 1f);
        }
        else
        {
            gunImage.localScale = new Vector3(1f, 1f, 1f);
        }

        // e‰æ‘œ‚ÌˆÊ’u‚ðŒÅ’è
        gunImage.localPosition = defaultLocalPos;
    }
}