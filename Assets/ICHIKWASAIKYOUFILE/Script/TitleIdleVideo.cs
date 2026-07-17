using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class TitleIdleVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject videoObject;

    public float idleTime = 10f;

    float timer = 0f;
    bool isPlaying = false;

    public RawImage rawImage;
    public float fadeTime = 1.0f;

    void Start()
    {
        videoObject.SetActive(false);
    }

    void Update()
    {
        // âΩÇ©ì¸óÕÇ™Ç†Ç¡ÇΩ
        if (Input.anyKeyDown ||
            Input.GetAxis("Mouse X") != 0 ||
            Input.GetAxis("Mouse Y") != 0)
        {
            timer = 0;

            if (isPlaying)
            {
                StopVideo();
            }

            return;
        }

        if (!isPlaying)
        {
            timer += Time.deltaTime;

            if (timer >= idleTime)
            {
                PlayVideo();
            }
        }
    }

    void PlayVideo()
    {
        isPlaying = true;

        videoObject.SetActive(true);

        // ç≈èâÇÕìßñæ
        Color c = rawImage.color;
        c.a = 0;
        rawImage.color = c;

        videoPlayer.Play();

        StartCoroutine(FadeIn());
    }

    void StopVideo()
    {
        isPlaying = false;
        videoPlayer.Stop();
        videoObject.SetActive(false);
    }

 

IEnumerator FadeIn()
{
    float t = 0;

    while (t < fadeTime)
    {
        t += Time.deltaTime;

        Color c = rawImage.color;
        c.a = Mathf.Lerp(0, 1, t / fadeTime);
        rawImage.color = c;

        yield return null;
    }

    Color color = rawImage.color;
    color.a = 1;
    rawImage.color = color;
}
}