using UnityEngine;

public class TrapController : MonoBehaviour
{
    [Header("爆発の設定")]
    public float explosionScale = 5.0f; // 爆発の大きさ
    public float explosionDuration = 0.5f; // 爆発が消えるまでの時間

    private bool hasExploded = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;

        // ★当たったオブジェクトから、一番上の親（ルート）まで遡って敵を探す
        Transform current = collision.transform;
        GameObject enemyRoot = null;

        while (current != null)
        {
            if (current.CompareTag("Enemy"))
            {
                enemyRoot = current.gameObject;
                break; // 敵のタグを見つけたらループを抜ける
            }
            current = current.parent; // 1個上の親へ進む
        }

        // 🚨 どこかの階層に「Enemy」タグがついていたら爆発！
        if (enemyRoot != null)
        {
            Explode(enemyRoot);
        }
    }

    void Explode(GameObject victim)
    {
        hasExploded = true;

        // --- 爆発エフェクトの生成 ---
        GameObject explosion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosion.transform.position = transform.position;
        explosion.GetComponent<Renderer>().material.color = Color.yellow;
        Destroy(explosion.GetComponent<Collider>());

        // 爆発アニメーションの追加
        ExplosionAnim anim = explosion.AddComponent<ExplosionAnim>();
        anim.maxScale = explosionScale;
        anim.duration = explosionDuration;

        // 敵（分身の親丸ごと）を消す
        Debug.Log(victim.name + " を爆発で一網打尽にしました！");
        Destroy(victim);

        // 罠自体を消す
        Destroy(gameObject);
    }
}

// 爆発の動きを制御するクラス（変更なし）
public class ExplosionAnim : MonoBehaviour
{
    public float maxScale;
    public float duration;
    private float timer = 0;

    void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / duration;
        transform.localScale = Vector3.one * (progress * maxScale);
        GetComponent<Renderer>().material.color = Color.Lerp(Color.yellow, Color.red, progress);

        if (timer >= duration) Destroy(gameObject);
    }
}