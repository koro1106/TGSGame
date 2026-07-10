using UnityEngine;


//タイトル画面管理

//　カーテンでないです

public class title : MonoBehaviour
{
    //[Header("カーテン")]
    //[SerializeField]
    //private CurtainSceneManager curtainManager;


    // スタートボタン
 
    public void OnStartButton()
    {
        //curtainManager.ChangeScene("MainStageScene");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("スタートボタンが押されました");

            //curtainManager.ChangeScene("MainStageScene");
        }
    }
}