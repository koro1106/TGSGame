using UnityEngine;
using TMPro;

public class ComboPopup : MonoBehaviour
{
    // 消えるまでの時間
    public float destroyTime = 0.8f;

    // TextMeshPro
    private TMP_Text textMesh;

    void Awake()
    {
        // StartじゃなくAwakeで取得するのが安全
        textMesh = GetComponentInChildren<TMP_Text>();
    }

    void Start()
    {
        // 0.8秒後に消す
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // 少し上に動かす
        transform.position += Vector3.up * Time.deltaTime;
    }

    // コンボ表示
    public void SetCombo(int combo)
    {
        if (textMesh == null)
        {
            Debug.LogError("TMP_Textが見つからない！");
            return;
        }

        textMesh.text = combo + " COMBO";
    }
}