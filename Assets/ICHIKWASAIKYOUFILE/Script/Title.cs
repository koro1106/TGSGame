using UnityEngine;


//タイトル画面管理

public class title : MonoBehaviour
{
    [Header("カーテン")]
    [SerializeField]
    private CurtainSceneManager curtainManager;


    // スタートボタン
 
    public void OnStartButton()
    {
        curtainManager.ChangeScene("ichikawa");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("スタートボタンが押されました");

            curtainManager.ChangeScene("MainScene");
        }
    }
}