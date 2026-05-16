using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class title : MonoBehaviour
{

    //　くりっくのやつ
    public void OnStartButton()
    {
        SceneManager.LoadScene("MainStageScene");
    }

    // この関数がボタンをクリックしたときに呼ばれます
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("スタートボタンが押されました");
            // 効果音を鳴らす


            // シーンをロード

            SceneManager.LoadScene("MainScene");

        }
    }
}