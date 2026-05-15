using UnityEngine;

public class Explosion : MonoBehaviour
{
    public ParticleSystem hitEffectPrefab; //　爆発物

    // 敵にぶつかったらパーティクルシステム再生してください
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Vector3 hitPos = collision.contacts[0].point;
            Quaternion rot = Quaternion.LookRotation(collision.contacts[0].normal);

            Instantiate(hitEffectPrefab, hitPos, rot);

            Destroy(gameObject); // 弾（このオブジェクト）を削除
        }
    }
}