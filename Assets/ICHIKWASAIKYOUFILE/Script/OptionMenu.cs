using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OptionMenu : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private MenuSelector menuSelector;
    [SerializeField] private SEManager seManager;

    public void OpenOption()
    {
        animator.SetTrigger("Open");
        seManager.PlayClickSE();
    }

    public void CloseOption()
    {
        animator.SetTrigger("Close");
        seManager.PlayBackSE();
    }
}