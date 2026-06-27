using UnityEngine;
using System.Collections;
using UnityEngine.UI; // UIを使う場合

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("ホットバー設定 (1:スプレー, 2:トラップ)")]
    public int currentWeapon = 1; // 初期状態は1（スプレー）

    [Header("スプレー設定")]
    public Transform sprayPoint;     // スプレーが出る位置（カメラの前などに空オブジェクトを配置）
    public float sprayRange = 3f;    // スプレーの届く距離
    public float sprayAngle = 30f;   // スプレーの放射角度

    [Header("トラップ設定")]
    public GameObject trapPrefab;    // 地面に置くトラップのプレハブ
    public Transform dropPoint;      // トラップを落とす位置（プレイヤーの足元前など）

    void Update()
    {
        // --- 1. ホットバーのキー切り替え ---
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentWeapon = 1;
            Debug.Log("スプレーを選択しました");
            // 💡ここにホットバーUIの色を変える処理などを入れると最高です
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentWeapon = 2;
            Debug.Log("トラップを選択しました");
        }

        // --- 2. マウスクリック（左クリック）で武器使用 ---
        if (Input.GetMouseButtonDown(0))
        {
            if (currentWeapon == 1)
            {
                UseSpray();
            }
            else if (currentWeapon == 2)
            {
                SpawnTrap();
            }
        }
    }

    // スプレーを発射する処理
    void UseSpray()
    {
        Debug.Log("スプレーを噴射！");
        // 💡ここにパーティクル（煙）を再生するコードを入れるとリアルになります

        // 前方の一定範囲にいる虫を探す
        Collider[] hitColliders = Physics.OverlapSphere(sprayPoint.position, sprayRange);
        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Enemy")) // 虫に「Enemy」タグをつけておいてください
            {
                Vector3 dirToEnemy = (hit.transform.position - sprayPoint.position).normalized;
                float angle = Vector3.Angle(sprayPoint.forward, dirToEnemy);

                // 扇型の範囲内に入っているかチェック
                if (angle <= sprayAngle)
                {
                    // 虫のスクリプトに「スプレーを浴びた」と通知する
                    BugMovement bug = hit.GetComponent<BugMovement>();
                    if (bug != null)
                    {
                        // ➔ 虫側のスクリプトに後述の「弱体化・死亡関数」を作って呼び出します
                        StartCoroutine(bug.TakeSprayDamage());
                    }
                }
            }
        }
    }

    // トラップを設置する処理
    void SpawnTrap()
    {
        if (trapPrefab != null && dropPoint != null)
        {
            Debug.Log("トラップを設置！");
            // 足元にトラップを生成
            Instantiate(trapPrefab, dropPoint.position, Quaternion.identity);
        }
    }
}