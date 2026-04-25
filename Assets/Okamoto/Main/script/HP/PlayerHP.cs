using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PlayerHP : MonoBehaviour
{
    public static PlayerHP Instance;

    public int maxHP = 100;
    public int currentHP;

    [Header("HPリセットシーン")]
    public string resetSceneName;

    [Header("UI")]
    public Image hpBar;          // 緑のバー
    public TMP_Text hpText;      // HPテキスト

    public Scrollbar hpScrollbar;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            currentHP = maxHP;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateHPUI(); // 最初に反映
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == resetSceneName)
        {
            currentHP = maxHP;
        }

        UpdateHPUI(); // シーン切替後も更新
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        UpdateHPUI(); // ←ここ重要
    }

    void UpdateHPUI()
    {
        float ratio = (float)currentHP / maxHP;

        // Scrollbar（逆転）
        if (hpScrollbar != null)
        {
            hpScrollbar.value = 1f - ratio;
        }

        // テキスト（％）
        if (hpText != null)
        {
            float percent = ratio * 100f;
            hpText.text = Mathf.CeilToInt(percent) + "%";
        }
    }
}