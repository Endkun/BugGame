using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 触れたオブジェクト、またはその親から RobotPlayerController を探す
        RobotPlayerController player = other.GetComponent<RobotPlayerController>();
        if (player == null && other.transform.parent != null)
        {
            player = other.transform.parent.GetComponent<RobotPlayerController>();
        }

        // ロボットが触れたなら鍵を獲得して消える
        if (player != null)
        {
            player.PickUpKey();
            Destroy(gameObject);
        }
    }
}