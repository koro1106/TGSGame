using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    private TextMeshPro text;

    public float moveSpeed = 2f;
    public float lifeTime = 1f;

    void Awake()
    {
        text = GetComponent<TextMeshPro>();
    }

    public void SetDamage(int damage)
    {
        text.text = damage.ToString();
    }

    void Update()
    {
        // è„Ç…ïÇÇ≠
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // éûä‘Ç≈è¡Ç¶ÇÈ
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }
}