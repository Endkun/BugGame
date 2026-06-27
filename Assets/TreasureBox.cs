using UnityEngine;

public class TreasureBox : MonoBehaviour
{
    [Header("消したいフタのオブジェクト (Bug_Headなどを指定)")]
    public GameObject boxHead;

    [Header("ドロップさせるアイテムのプレハブ (鍵など)")]
    public GameObject itemPrefab;

    private bool isOpened = false;

    private void OnTriggerEnter(Collider other)
    {
        CheckAndOpen(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckAndOpen(collision.gameObject);
    }

    void CheckAndOpen(GameObject hitObject)
    {
        if (isOpened) return;

        // プレイヤー（robot）の判定
        bool isPlayer = hitObject.name.Contains("robot") ||
                        (hitObject.transform.parent != null && hitObject.transform.parent.name.Contains("robot"));

        if (isPlayer)
        {
            isOpened = true;

            // 1. フタを消す
            if (boxHead != null) Destroy(boxHead);

            // 2. 宝箱の下（底）からくるくる浮き上がらせる
            if (itemPrefab != null)
            {
                // 【変更ポイント①】開始位置を宝箱の「底（少し下）」に設定
                // yの値を少し下げることで、箱の底から湧き出る演出になります
                Vector3 startPos = transform.position + Vector3.up * -0.1f;

                GameObject spawnedItem = Instantiate(itemPrefab, startPos, Quaternion.identity);

                // 【変更ポイント②】目標地点をプレイヤーから見えやすい高さ（少し手前・少し上）に計算
                Vector3 targetPos = transform.position + Vector3.up * 1.5f + transform.forward * 0.5f;

                // アイテム側に「くるくる浮き上がる動き」を命令するスクリプトをその場で追加
                FloatUpEffect floatEffect = spawnedItem.AddComponent<FloatUpEffect>();
                floatEffect.targetPosition = targetPos;

                // 回転や移動のスピードをここでお好みに調整も可能です
                floatEffect.floatSpeed = 2.5f;   // 少し早めにふわっと上がる
                floatEffect.rotateSpeed = 360f;  // 1秒間に1回転（くるくる感をアップ）
            }
        }
    }
}

// アイテムをふわっと浮かせて、その場でクルクル回転させる演出用クラス
public class FloatUpEffect : MonoBehaviour
{
    public Vector3 targetPosition;
    public float floatSpeed = 2.0f;     // 浮き上がるスピード
    public float rotateSpeed = 60.0f;    // クルクル回るスピード

    private bool reached = false;

    void Update()
    {
        // 1. 浮いている間（移動中も移動後も）、ずっとY軸を中心にクルクル回転させる
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        // 2. 目標地点まで滑らかに移動（浮き上がる）
        if (!reached)
        {
            // Lerpの特性上、目標に近づくほどゆっくりになります（じわっと止まる綺麗な動きになります）
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * floatSpeed);

            // ほぼ目標地点についたら移動終了
            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                transform.position = targetPosition;
                reached = true;
                OnAnimationComplete();
            }
        }
    }

    private void OnAnimationComplete()
    {
        Debug.Log("鍵が宝箱の上に出てきました！");
        // ここに「プレイヤーが拾えるようにコライダーを有効化する」などの処理を入れることもできます
    }
}