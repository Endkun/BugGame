using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        CheckAndOpen(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckAndOpen(other.gameObject);
    }

    private void CheckAndOpen(GameObject hitObject)
    {
        // 触れたオブジェクトから RobotPlayerController を探す
        RobotPlayerController player = hitObject.GetComponent<RobotPlayerController>();
        if (player == null && hitObject.transform.parent != null)
        {
            player = hitObject.transform.parent.GetComponent<RobotPlayerController>();
        }

        // ロボットが鍵を持っているかチェック
        if (player != null && player.hasKey)
        {
            player.UseKey(); // 鍵を消費

            Debug.Log("扉が開きました！");
            Destroy(gameObject); // ドアを消去
        }
        else if (player != null && !player.hasKey)
        {
            Debug.Log("鍵がないので開かないようだ。");
        }
    }
}