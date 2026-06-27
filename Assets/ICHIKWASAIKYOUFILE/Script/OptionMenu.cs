using UnityEngine;

public class OptionMenu : MonoBehaviour
{
    public Animator animator;

    public void OpenOption()
    {
        animator.SetTrigger("Open");
    }

    public void CloseOption()
    {
        animator.SetTrigger("Close");
    }
}