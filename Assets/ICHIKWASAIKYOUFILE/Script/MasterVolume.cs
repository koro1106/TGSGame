using UnityEngine;
using UnityEngine.UI;

public class MasterVolume : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;

    private void Start()
    {
        // 保存されている音量を読み込む（初回は1.0）
        float volume = PlayerPrefs.GetFloat("MasterVolume", 1f);

        AudioListener.volume = volume;
        volumeSlider.value = volume;

        // スライダーが動いたら音量変更
        volumeSlider.onValueChanged.AddListener(ChangeVolume);
    }

    private void ChangeVolume(float value)
    {
        AudioListener.volume = value;

        // 保存
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }
}