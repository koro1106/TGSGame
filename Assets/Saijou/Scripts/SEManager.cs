using UnityEngine;
/// <summary>
/// SEä«óùóp
/// </summary>
public class SEManager : MonoBehaviour
{
    public static SEManager Instance;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip levelUpSE;
    [SerializeField] private AudioClip shootSE;
    [SerializeField] private AudioClip backSE;
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void SetSEVolume(float value)
    {
        audioSource.volume = value;
    }

    public float GetSEVolume()
    {
        return audioSource.volume;
    }
    public void PlayLevelUpSE()
    {
        audioSource.PlayOneShot(levelUpSE);
    }
    public void PlayShootSE()
    {
        audioSource.PlayOneShot(shootSE);
    }
    public void PlayBackSE()
    {
        audioSource.PlayOneShot(backSE);
    }
}

