using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    [Header("İ’è")]
    public float duration = 0.3f;
    public float strength = 20f;

    private float timer;
    private Vector3 originalPos;

    // ƒŠƒUƒ‹ƒg‚È‚Ç‚Å‹­§’â~‚·‚é‚½‚ß
    private bool stopped = false;


    void Awake()
    {
        Instance = this;

        originalPos = transform.localPosition;
    }


    void LateUpdate()
    {
        // =====================================================
        // ‹­§’â~’†
        // =====================================================

        if (stopped)
        {
            transform.localPosition = originalPos;
            return;
        }


        // =====================================================
        // ƒ|[ƒY’†
        // =====================================================

        if (PauseMenu.IsPaused)
        {
            transform.localPosition = originalPos;
            return;
        }


        // =====================================================
        // ƒJƒƒ‰—h‚ê
        // =====================================================

        if (timer > 0)
        {
            float damper =
                timer / duration;

            float x =
                Random.Range(-1f, 1f)
                * strength
                * damper;

            float y =
                Random.Range(-1f, 1f)
                * strength
                * damper;

            transform.localPosition =
                originalPos +
                new Vector3(x, y, 0);

            timer -= Time.deltaTime;
        }
        else
        {
            transform.localPosition =
                originalPos;
        }
    }


    // =========================================================
    // ’Êí‚Ì—h‚ê
    // =========================================================

    public void Shake()
    {
        if (stopped)
            return;

        timer = duration;
    }


    public void Shake(
        float _duration,
        float _strength)
    {
        if (stopped)
            return;

        duration = _duration;
        strength = _strength;

        timer = duration;
    }


    // =========================================================
    // ƒJƒƒ‰—h‚ê‚ğ’â~
    // =========================================================

    public void StopShake()
    {
        stopped = true;

        timer = 0f;

        transform.localPosition =
            originalPos;
    }


    // =========================================================
    // ƒJƒƒ‰—h‚ê‚ğÄŠJ
    // =========================================================

    public void ResumeShake()
    {
        stopped = false;

        timer = 0f;

        transform.localPosition =
            originalPos;
    }
}