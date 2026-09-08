using UnityEngine;

public class ResultDestroyEffect : MonoBehaviour
{
    private void OnEnable()
    {
        ResultManager.RegisterEffect(gameObject);
    }

    private void OnDestroy()
    {
        ResultManager.UnregisterEffect(gameObject);
    }
}