using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// ¶‰º‚¨ì—l‰æ‘œØ‘Ö—p
/// </summary>
public class UISwitch : MonoBehaviour
{
    public GameObject image1;
    public GameObject image2;

    private void Start()
    {
        StartCoroutine(SwitchImage());
    }

    IEnumerator SwitchImage()
    {
        while (true)
        {
            image1.SetActive(true);
            image2.SetActive(false);
            yield return new WaitForSeconds(1.25f);

            image1.SetActive(false);
            image2.SetActive(true);
            yield return new WaitForSeconds(1.25f);
        }
    }
}
