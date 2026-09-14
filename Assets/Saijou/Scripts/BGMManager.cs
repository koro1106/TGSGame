using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetBGMVolume(float value)
    {
        audioSource.volume = value;
    }

    public float GetBGMVolume()
    {
        return audioSource.volume;
    }
}
