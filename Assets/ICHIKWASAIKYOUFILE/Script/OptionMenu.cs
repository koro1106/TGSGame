using UnityEngine;

public class OptionMenu : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private MenuSelector menuSelector;

    public void OpenOption()
    {
        animator.SetTrigger("Open");
    }

    public void CloseOption()
    {
        animator.SetTrigger("Close");
    }
}