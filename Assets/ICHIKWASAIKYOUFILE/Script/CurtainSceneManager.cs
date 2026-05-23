using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class CurtainSceneManager : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private float closeTime = 1f;

    private static CurtainSceneManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        transform.SetParent(null);

        DontDestroyOnLoad(gameObject);
    }

    public void ChangeScene(string sceneName)
    {
        StartCoroutine(ChangeSceneRoutine(sceneName));
    }

    private IEnumerator ChangeSceneRoutine(string sceneName)
    {
        // カーテン閉じる
        animator.SetTrigger("Close");

        yield return new WaitForSeconds(closeTime);

        // シーン移動
        SceneManager.LoadScene(sceneName);

        yield return null;

        // カーテン開く
        animator.SetTrigger("Open");
    }
}