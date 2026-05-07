using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    [Header("Ý’è")]
    public float duration = 0.3f;
    public float strength = 20f; // © ‰æ–Ê—h‚ê‚¾‚©‚ç‘å‚«‚ß‚ÅOK

    private float timer;
    private Vector3 originalPos;

    void Awake()
    {
        Instance = this;
        originalPos = transform.localPosition;
    }

    void LateUpdate()
    {
        if (PauseMenu.IsPaused)
        {
            transform.localPosition = originalPos;
            return;
        }

        if (timer > 0)
        {
            float damper = timer / duration;

            float x = Random.Range(-1f, 1f) * strength * damper;
            float y = Random.Range(-1f, 1f) * strength * damper;

            transform.localPosition = originalPos + new Vector3(x, y, 0);

            timer -= Time.deltaTime;
        }
        else
        {
            transform.localPosition = originalPos;
        }
    }

    public void Shake()
    {
        timer = duration;
    }

    public void Shake(float _duration, float _strength)
    {
        duration = _duration;
        strength = _strength;
        timer = duration;
    }

}