using UnityEngine;
using System.Collections;

public class BugMovement : MonoBehaviour
{
    [Header("ターゲット（プレイヤー）設定")]
    public Transform playerTarget;
    public float targetDetectionRange = 10f; // 10マス以内で追跡

    [Header("自爆·分裂設定")]
    public float explodeRange = 3.5f;
    public GameObject bugPrefab;
    public bool isMiniBug = false;
    private bool isExploding = false;

    [Header("張り付き·体液スプラッシュ設定")]
    public float attachRange = 1.2f;        // ミニ虫がプレイヤーに張り付く距離
    private bool isAttached = false;       // 張り付き中フラグ
    public int splashLiquidCount = 15;     // 飛び散る体液の数

    [Header("移動·ジャンプ速度設定")]
    public float moveSpeed = 6f;
    public float chaseSpeed = 10f;
    public float jumpForce = 5.5f;
    public float decisionInterval = 0.35f;

    [Header("脚の関節（8本分）")]
    public Transform[] legJoints;
    public float legWaveSpeedNormal = 25f;
    public float legWaveSpeedAir = 55f;
    public float legWaveAngle = 20f;

    [Header("アンテナ（2本）")]
    public Transform antenna0;
    public Transform antenna1;
    public float antennaWaveSpeed = 18f;
    public float antennaWaveAngle = 20f;

    [Header("リアルな伸縮モーション用")]
    public Transform visualBody;

    private Rigidbody rb;
    private Collider bugCollider;
    private bool isGrounded;
    private bool isChasing = false;
    private Vector3 originalScale;
    private bool isDead = false; // 死亡フラグ

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        bugCollider = GetComponent<Collider>();

        if (visualBody != null) originalScale = visualBody.localScale;
        else originalScale = transform.localScale;

        if (isMiniBug)
        {
            chaseSpeed = 12f;
            decisionInterval = 0.15f;
        }

        StartCoroutine(AdvancedJumpRoutine());
    }

    void Update()
    {
        if (isExploding || isDead) return;

        if (isAttached)
        {
            AnimateLimbsAndBody();
            return;
        }

        CheckForPlayerDistance();
        AnimateLimbsAndBody();
    }

    void FixedUpdate()
    {
        if (isAttached || isDead) return;
        // エラー回避のため、古いUnityでも動く「rb.velocity」に統一しています
        isGrounded = Mathf.Abs(rb.velocity.y) < 0.1f;
    }

    void CheckForPlayerDistance()
    {
        if (playerTarget == null) return;

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (isMiniBug && distance <= attachRange && !isAttached)
        {
            AttachToPlayer();
            return;
        }

        if (!isMiniBug && distance <= explodeRange && !isExploding)
        {
            StartCoroutine(ExplosionRoutine());
            return;
        }

        if (distance <= targetDetectionRange) isChasing = true;
        else isChasing = false;
    }

    void AttachToPlayer()
    {
        isAttached = true;
        isChasing = false;

        if (rb != null) Destroy(rb);
        if (bugCollider != null) bugCollider.enabled = false;

        transform.SetParent(playerTarget);
        transform.localPosition = new Vector3(Random.Range(-0.2f, 0.2f), 0.5f, 0.3f);
        transform.localRotation = Quaternion.Euler(Random.Range(-20, 20), 180, 0);

        legWaveSpeedNormal = 90f;
        legWaveAngle = 35f;

        Debug.Log("虫がプレイヤーに張り付いた！");
    }

    IEnumerator ExplosionRoutine()
    {
        isExploding = true;
        isChasing = false;
        legWaveSpeedNormal = 80f;

        float duration = 1.0f;
        float elapsed = 0f;

        Renderer renderer = GetComponentInChildren<Renderer>();
        Color originalColor = renderer != null ? renderer.material.color : Color.white;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.localScale = originalScale * Mathf.Lerp(1f, 1.6f, t);
            if (renderer != null) renderer.material.color = Color.Lerp(originalColor, Color.red, t);

            yield return null;
        }

        SpawnFluidSplash();

        if (bugPrefab != null)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 spawnOffset = new Vector3(Random.Range(-0.6f, 0.6f), 0.5f, Random.Range(-0.6f, 0.6f));
                GameObject miniBug = Instantiate(bugPrefab, transform.position + spawnOffset, Quaternion.identity);

                miniBug.name = "Mini_Bug_" + i;
                miniBug.transform.localScale = originalScale * 0.33f;

                BugMovement miniBugScript = miniBug.GetComponent<BugMovement>();
                if (miniBugScript != null)
                {
                    miniBugScript.playerTarget = this.playerTarget;
                    miniBugScript.isMiniBug = true;
                }

                Rigidbody miniRb = miniBug.GetComponent<Rigidbody>();
                if (miniRb != null)
                {
                    Vector3 forceDir = new Vector3(Random.Range(-1f, 1f), 1.2f, Random.Range(-1f, 1f)).normalized;
                    miniRb.AddForce(forceDir * 9f, ForceMode.VelocityChange);
                }
            }
        }

        Destroy(gameObject);
    }

    void SpawnFluidSplash()
    {
        for (int i = 0; i < splashLiquidCount; i++)
        {
            GameObject liquid = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            liquid.name = "Bug_Blood";
            liquid.transform.position = transform.position + Vector3.up * 0.5f;
            liquid.transform.localScale = Vector3.one * Random.Range(0.2f, 0.5f);

            Renderer liqRend = liquid.GetComponent<Renderer>();
            if (liqRend != null)
            {
                liqRend.material.color = new Color(0f, Random.Range(0.6f, 1f), 0f);
            }

            Rigidbody liqRb = liquid.AddComponent<Rigidbody>();
            Vector3 throwDir = new Vector3(Random.Range(-1f, 1f), Random.Range(0.5f, 1.5f), Random.Range(-1f, 1f)).normalized;
            liqRb.AddForce(throwDir * Random.Range(5f, 12f), ForceMode.VelocityChange);

            Destroy(liquid, 5f);
        }
    }

    IEnumerator AdvancedJumpRoutine()
    {
        while (true)
        {
            if (isExploding || isAttached || isDead) yield break;

            if (isGrounded)
            {
                float shrinkTime = 0.12f;
                float elapsed = 0f;
                while (elapsed < shrinkTime)
                {
                    if (isExploding || isAttached || isDead) yield break;
                    SetBodyScale(new Vector3(originalScale.x * 1.2f, originalScale.y * 0.5f, originalScale.z * 1.2f));
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                float currentSpeed = moveSpeed;

                if (isChasing && playerTarget != null)
                {
                    Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
                    directionToPlayer.y = 0;
                    transform.forward = directionToPlayer;
                    currentSpeed = chaseSpeed;
                }
                else
                {
                    float randomAngle = Random.Range(0f, 360f);
                    transform.rotation = Quaternion.Euler(0, randomAngle, 0);
                }

                Vector3 jumpDir = (transform.forward * currentSpeed) + (Vector3.up * jumpForce);
                rb.AddForce(jumpDir, ForceMode.VelocityChange);

                SetBodyScale(new Vector3(originalScale.x * 0.8f, originalScale.y * 1.5f, originalScale.z * 0.8f));
                yield return new WaitForSeconds(0.08f);

                SetBodyScale(originalScale);
            }

            float currentInterval = isChasing ? decisionInterval * 0.4f : decisionInterval;
            yield return new WaitForSeconds(Random.Range(currentInterval * 0.8f, currentInterval * 1.2f));
        }
    }

    void AnimateLimbsAndBody()
    {
        float time = Time.time;
        float currentLegSpeed = isGrounded ? legWaveSpeedNormal : legWaveSpeedAir;

        if (legJoints != null)
        {
            for (int i = 0; i < legJoints.Length; i++)
            {
                if (legJoints[i] != null)
                {
                    float phaseShift = isGrounded ? (i * 1.5f) : (i * 3.0f);
                    float wave = Mathf.Sin(time * currentLegSpeed + phaseShift) * legWaveAngle;
                    float finalAngle = isChasing || isAttached ? wave * 1.3f : wave;
                    legJoints[i].localRotation = Quaternion.Euler(finalAngle, 0, finalAngle * 0.5f);
                }
            }
        }

        float currentAntennaSpeed = isChasing || isAttached ? antennaWaveSpeed * 2.5f : antennaWaveSpeed;
        float currentAntennaAngle = isChasing || isAttached ? antennaWaveAngle * 0.4f : antennaWaveAngle;

        if (antenna0 != null)
        {
            float waveA = Mathf.Sin(time * currentAntennaSpeed) * currentAntennaAngle;
            antenna0.localRotation = Quaternion.Euler(waveA, waveA * 0.3f, 0);
        }
        if (antenna1 != null)
        {
            float waveB = Mathf.Sin(time * currentAntennaSpeed + Mathf.PI) * currentAntennaAngle;
            antenna1.localRotation = Quaternion.Euler(waveB, waveB * -0.3f, 0);
        }
    }

    void SetBodyScale(Vector3 targetScale)
    {
        if (visualBody != null) visualBody.localScale = targetScale;
        else transform.localScale = targetScale;
    }

    // ========== 🪓 以下、新ギミック用の追加関数 ==========

    // 🕸️ 1. トラップにハマったときの処理
    public void GetTrapped(float duration)
    {
        if (isDead || isExploding || isAttached) return;
        StartCoroutine(TrapRoutine(duration));
    }

    IEnumerator TrapRoutine(float duration)
    {
        Debug.Log("虫：トラップにハマって動けない！");
        float previousSpeed = moveSpeed;
        float previousChase = chaseSpeed;

        moveSpeed = 0;
        chaseSpeed = 0;
        if (rb != null) rb.velocity = Vector3.zero; // その場にピタッと止める
        legWaveSpeedNormal = 5f; // 足がのろのろ動く

        yield return new WaitForSeconds(duration);

        // トラップ時間が終わったら復帰
        moveSpeed = previousSpeed;
        chaseSpeed = previousChase;
        legWaveSpeedNormal = 25f;
    }

    // 💨 2. スプレーを浴びたときの処理
    public IEnumerator TakeSprayDamage()
    {
        if (isDead) yield break;
        isDead = true;

        Debug.Log("虫：スプレーを浴びて死亡した！");

        // ひっくり返って死ぬ演出（回転固定を解除して吹き飛ばす）
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.None;
            rb.AddForce(Vector3.up * 4f + transform.right * 2f, ForceMode.VelocityChange);
        }

        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    // 🪨 3. 石（球体）に潰されたときの処理
    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        // ぶつかってきた物の質量が100以上（重い石）だったら
        if (collision.rigidbody != null && collision.rigidbody.mass >= 100f)
        {
            // 上から降ってきたとき
            if (collision.contacts[0].normal.y < -0.4f)
            {
                isDead = true;
                Debug.Log("虫：グシャッ！岩に潰された！");

                // 横に平べったく潰す
                transform.localScale = new Vector3(originalScale.x * 1.8f, originalScale.y * 0.1f, originalScale.z * 1.8f);

                moveSpeed = 0;
                chaseSpeed = 0;
                if (rb != null) rb.isKinematic = true; // 物理を止めて完全に固定

                Destroy(gameObject, 1.5f);
            }
        }
    }
}