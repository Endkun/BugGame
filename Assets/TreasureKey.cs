using UnityEngine;

public class TreasureKey : MonoBehaviour
{
    [Header("移動の設定")]
    [SerializeField] private float riseHeight = 2.0f;  // 上昇する高さ
    [SerializeField] private float duration = 1.5f;    // 上昇にかかる時間

    [Header("回転の設定")]
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0, 360, 0); // 1秒間の回転角度

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float elapsedTime = 0f;
    private bool isAnimating = false;

    // 演出を開始するメソッド（宝箱を開けるスクリプトから呼ぶ）
    public void StartAnimate()
    {
        startPosition = transform.position;
        targetPosition = startPosition + Vector3.up * riseHeight;
        elapsedTime = 0f;
        isAnimating = true;

        // 最初は非表示にしていた場合はここで表示する
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!isAnimating) return;

        // 1. くるくる回す（回転）
        transform.Rotate(rotationSpeed * Time.deltaTime);

        // 2. 上に上がっていく（移動）
        elapsedTime += Time.deltaTime;
        float t = elapsedTime / duration;

        // イージング（じわっと動いてピタッと止まる滑らかな動き）
        t = Mathf.SmoothStep(0f, 1f, t);

        transform.position = Vector3.Lerp(startPosition, targetPosition, t);

        // 指定の時間に達したらアニメーション終了
        if (elapsedTime >= duration)
        {
            transform.position = targetPosition;
            isAnimating = false;
            OnAnimationComplete();
        }
    }

    // アニメーションが終わった後の処理（プレイヤーが入手可能にするなど）
    private void OnAnimationComplete()
    {
        Debug.Log("鍵の出現アニメーションが完了しました！");
    }
}