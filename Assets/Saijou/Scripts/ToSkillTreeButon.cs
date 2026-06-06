using UnityEngine;
using UnityEngine.SceneManagement;

public class ToSkillTreeButon : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SceneManager.LoadScene("MainStageSkillTreeScene");
        }
    }
}
