using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class titleichikawa : MonoBehaviour
{

    //　くりっくのやつ
    public void OnStartButton()
    {
        SceneManager.LoadScene("WATAKUSHI");
    }

    // この関数がボタンをクリックしたときに呼ばれます
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("スタートボタンが押さたにょ");
            // 効果音を鳴らす


            // シーンをロード

            SceneManager.LoadScene("WATAKUSHI");

        }
    }
}