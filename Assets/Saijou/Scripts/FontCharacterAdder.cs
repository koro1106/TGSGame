using TMPro;
using UnityEngine;

public class FontCharacterAdder : MonoBehaviour
{
    [SerializeField] TMP_FontAsset fontAsset;

    void Start()
    {
        string characters = "不なを別きます";

        bool success = fontAsset.TryAddCharacters(
            characters,
            out string missingCharacters
        );

        Debug.Log("追加結果: " + success);
        Debug.Log("追加できなかった文字: " + missingCharacters);
    }
}
