using UnityEngine;

public class BugDeath : MonoBehaviour
{
    // 物体が衝突したときに自動で呼ばれる関数
    private void OnCollisionEnter(Collision collision)
    {
        // ぶつかってきた相手の名前が「Trap」か、名前に「Sphere（球）」が含まれている場合
        if (collision.gameObject.name == "Trap" || collision.gameObject.name.Contains("rock"))
        {
            // 虫の悲鳴やエフェクトをここに入れることも可能
            Debug.Log("虫が踏み潰されました！");

            // 虫オブジェクト（自分自身）を削除
            Destroy(gameObject);
        }
    }
}