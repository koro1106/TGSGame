using UnityEngine;
/// <summary>
/// SEä«óùóp
/// </summary>
public class SEManager : MonoBehaviour
{
    public static SEManager Instance;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip levelUpSE;
    private void Awake()
    {
        Instance = this;
    }
    public void PlayLevelUpSE()
    {
        audioSource.PlayOneShot(levelUpSE);
    }
}

