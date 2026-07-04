using UnityEngine;
using UnityEngine.UI;

public class MultiImageSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class ImageData
    {
        public Image targetImage;
        public Sprite[] sprites;

        [HideInInspector] public int currentIndex;
    }

    [Header("‰æ‘œ1")]
    public ImageData image1;

    [Header("‰æ‘œ2")]
    public ImageData image2;

    [Header("‰æ‘œ3")]
    public ImageData image3;

    [Header("Ø‚è‘Ö‚¦ŠÔŠu")]
    public float changeInterval = 0.2f;

    private float timer;

    void Start()
    {
        Init(image1);
        Init(image2);
        Init(image3);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= changeInterval)
        {
            timer = 0f;

            SwitchImage(image1);
            SwitchImage(image2);
            SwitchImage(image3);
        }
    }

    void Init(ImageData data)
    {
        if (data.targetImage != null && data.sprites.Length > 0)
        {
            data.currentIndex = 0;
            data.targetImage.sprite = data.sprites[0];
        }
    }

    void SwitchImage(ImageData data)
    {
        if (data.targetImage == null || data.sprites.Length <= 1)
            return;

        data.currentIndex++;

        if (data.currentIndex >= data.sprites.Length)
            data.currentIndex = 0;

        data.targetImage.sprite = data.sprites[data.currentIndex];
    }
}