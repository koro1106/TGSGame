using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public void OnPlayScene() // プレイシーンに戻る
    {
        SceneManager.LoadScene("SampleScene");
    }
}
