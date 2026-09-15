using UnityEngine;
using System.Collections;

public class ShowImage : MonoBehaviour
{
    public GameObject imageObject;
    public float displayTime = 3f;

    public void Show()
    {
        imageObject.SetActive(true);
        StartCoroutine(HideImage());
    }

    IEnumerator HideImage()
    {
        yield return new WaitForSeconds(displayTime);

        imageObject.SetActive(false);
    }
}