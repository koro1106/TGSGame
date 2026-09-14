using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    public Slider sensiSlider;
    public Slider volumeSlider;
    public Slider seSlider;

    void Start()
    {
        // BGM
        if (BGMManager.Instance != null)
        {
            volumeSlider.value = BGMManager.Instance.GetBGMVolume();
            volumeSlider.onValueChanged.AddListener(ChangeBGMVolume);
        }
        // SE‰¹—Ê
        if (SEManager.Instance != null)
        {
            seSlider.value = SEManager.Instance.GetSEVolume();
            seSlider.onValueChanged.AddListener(ChangeSEVolume);
        }
    }
    void ChangeBGMVolume(float value)
    {
        BGMManager.Instance.SetBGMVolume(value);
    }
    void ChangeSEVolume(float value)
    {
        if (SEManager.Instance != null)
        {
            SEManager.Instance.SetSEVolume(value);
        }
    }
}