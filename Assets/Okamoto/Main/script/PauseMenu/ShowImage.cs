using UnityEngine;

public class ShowImage : MonoBehaviour
{
    public GameObject targetImage;

    public void OnClickButton()
    {
        targetImage.SetActive(true);
    }
}