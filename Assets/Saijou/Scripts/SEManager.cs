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
    [SerializeField] private AudioClip clickSE;
    [SerializeField] private AudioClip collectSE;
    [SerializeField] private AudioClip openSE;

    //íe
    [SerializeField] private AudioClip bindHitSE;//çΩ
    [SerializeField] private AudioClip explosionHitSE;//îöî≠
    [SerializeField] private AudioClip poisonHitSE;//ì≈
    [SerializeField] private AudioClip gravityHitSE;//èdóÕ
    [SerializeField] private AudioClip chainHitSE;//óã

    // ìGíÖíeSE
    [SerializeField] private AudioClip enemyHitSE;//í èÌ

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

    public void PlayClickSE()
    {
        audioSource.PlayOneShot(clickSE);
    }

    public void PlayCollectSE()
    {
        audioSource.PlayOneShot(collectSE);
    }

    public void PlayOpenSE()
    {
        audioSource.PlayOneShot(openSE);
    }

    // ìGíÖíeSE
    public void PlayEnemyHitSE()
    {
        audioSource.PlayOneShot(enemyHitSE);
    }
    public void PlayBindHitSE()
    {
        audioSource.PlayOneShot(bindHitSE);
    }
    public void PlayExplosionHitSE()
    {
        audioSource.PlayOneShot(explosionHitSE);
    }
    public void PlayPoisonHitSE()
    {
        audioSource.PlayOneShot(poisonHitSE);
    }
    public void PlayGravityHitSE()
    {
        audioSource.PlayOneShot(gravityHitSE);
    }
    public void PlayChainHitSE()
    {
        audioSource.PlayOneShot(chainHitSE);
    }
}