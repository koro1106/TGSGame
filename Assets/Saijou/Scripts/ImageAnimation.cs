using UnityEngine;
using UnityEngine.UI;

public class ImageAnimation : MonoBehaviour
{
    public Sprite[] frames;
    public float frameRate = 12f;

    private Image image;
    private int currentFrame = 0;
    private float timer = 0f;

    void Start()
    {
        image = GetComponent<Image>();

        if (frames.Length > 0)
        {
            image.sprite = frames[0];
        }
    }

    void Update()
    {
        if (frames.Length == 0) return;

        timer += Time.deltaTime;

        if (timer >= 1f / frameRate)
        {
            timer = 0f;

            currentFrame++;

            if (currentFrame >= frames.Length)
            {
                currentFrame = 0;
            }

            image.sprite = frames[currentFrame];
        }
    }
}
