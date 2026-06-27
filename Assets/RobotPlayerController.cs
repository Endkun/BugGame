using UnityEngine;

public class RobotPlayerController : MonoBehaviour
{
    [Header("移動設定（慣性付き）")]
    public float walkSpeed = 3.0f;
    public float runSpeed = 7.0f;
    public float backwardSpeed = 2.0f;
    public float rotateSpeed = 120.0f;
    public float jumpForce = 6.0f;
    [Tooltip("停止時の滑り具合（大きいほどすぐ止まる）")]
    public float stoppingDamping = 4.0f;

    [Header("モデルの向き微調整")]
    public float modelRotationOffset = 90f;

    [Header("リアルアニメーション設定")]
    public float walkSwingSpeed = 6.0f;
    public float runSwingSpeed = 12.0f;
    public float baseSwingAngle = 35.0f;
    public float runSwingAngleMultiplier = 1.4f;
    public float bodyBobHeight = 0.12f;
    public float walkLeanAngle = 5.0f;
    public float runLeanAngle = 18.0f;

    [Header("余韻・慣性設定")]
    public float swingSettleSpeed = 3.0f;
    public float idleBreathingSpeed = 2.0f;

    [Header("手足・モデルの参照")]
    public Transform rArm;
    public Transform lArm;
    public Transform rLeg;
    public Transform lLeg;
    public Transform bodyVisual;

    [Header("罠（Trap）の生成設定")]
    [Tooltip("Assetsから罠のプレハブをここに登録してください")]
    public GameObject trapPrefab;

    [Header("【新規】インベントリ設定")]
    [Tooltip("現在鍵を持っているかどうか")]
    public bool hasKey = false; // 先ほどのPlayerInventoryの役割をここに統合しました

    private Rigidbody rb;
    private Vector3 initialBodyPosition;
    private Quaternion initialBodyRotation;
    private float animationTimer;
    private bool isJumping = false;

    // 余韻計算用の変数
    private float currentSwingAmplitude = 0f;
    private float currentBodyLean = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (bodyVisual != null)
        {
            initialBodyPosition = bodyVisual.localPosition;
            initialBodyRotation = bodyVisual.localRotation;
        }

        // 物理演算による勝手な回転を完全にブロック
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        rb.drag = 0.5f;
    }

    void Update()
    {
        // スペースキーでジャンプ
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        // 2キーを押したら足元に罠を設置
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SpawnTrap();
        }

        // 【削除】3キーのパーツセパレーター処理は削除しました

        if (isJumping && Mathf.Abs(rb.velocity.y) < 0.01f)
        {
            isJumping = false;
        }
    }

    void FixedUpdate()
    {
        // 1. 旋回（回転）処理
        float turnInput = Input.GetAxisRaw("Horizontal");
        float turnAmount = turnInput * rotateSpeed * Time.fixedDeltaTime;
        transform.Rotate(0, turnAmount, 0);

        // 2. 前後移動の入力
        float moveInput = Input.GetAxisRaw("Vertical");

        // スティックの遊び（ドリフト）対策
        bool isMoving = Mathf.Abs(moveInput) > 0.15f;
        bool isTurning = Mathf.Abs(turnInput) > 0.15f;

        bool isRunning = (moveInput > 0.15f) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

        float targetSwingAmplitude = 0f;
        float targetLean = 0f;
        float animationSpeedMultiplier = 1f;

        if (isMoving)
        {
            Vector3 moveDirection = transform.TransformDirection(Quaternion.Euler(0, -modelRotationOffset, 0) * Vector3.forward);

            float speed = walkSpeed;
            targetLean = walkLeanAngle;

            if (isRunning)
            {
                speed = runSpeed;
                targetSwingAmplitude = runSwingAngleMultiplier;
                targetLean = runLeanAngle;
                animationSpeedMultiplier = runSwingSpeed / walkSwingSpeed;
            }
            else if (moveInput < -0.15f)
            {
                speed = backwardSpeed;
                targetLean = -walkLeanAngle * 0.5f;
            }
            else
            {
                targetSwingAmplitude = 1.0f;
            }

            Vector3 targetVelocity = moveDirection * moveInput * speed;
            rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);

            if (!isJumping)
            {
                animationTimer += Time.fixedDeltaTime * walkSwingSpeed * animationSpeedMultiplier * Mathf.Sign(moveInput);
            }
        }
        else
        {
            Vector3 currentVelocity = rb.velocity;
            Vector3 targetVelocity = new Vector3(0, currentVelocity.y, 0);
            rb.velocity = Vector3.Lerp(currentVelocity, targetVelocity, stoppingDamping * Time.fixedDeltaTime);

            if (rb.velocity.magnitude < 0.05f)
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
            }

            targetSwingAmplitude = 0f;
            targetLean = 0f;

            if (isTurning && !isJumping)
            {
                animationTimer += Time.fixedDeltaTime * walkSwingSpeed * 0.5f;
                targetSwingAmplitude = 0.5f;
            }
        }

        currentSwingAmplitude = Mathf.Lerp(currentSwingAmplitude, targetSwingAmplitude, Time.fixedDeltaTime * swingSettleSpeed);
        currentBodyLean = Mathf.Lerp(currentBodyLean, targetLean, Time.fixedDeltaTime * swingSettleSpeed);

        AnimateRobot(animationTimer);
    }

    void Jump()
    {
        if (!isJumping)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            isJumping = true;
            currentSwingAmplitude = 0.8f;
        }
    }

    // 罠を生成する関数
    void SpawnTrap()
    {
        if (trapPrefab != null)
        {
            Vector3 spawnPosition = transform.position + Vector3.up * 1.2f;
            Instantiate(trapPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("罠を設置しました！");
        }
        else
        {
            Debug.LogWarning("Trap Prefab がインスペクターで設定されていません！");
        }
    }

    // 【新規】鍵を拾った・使った時の管理メソッド
    public void PickUpKey()
    {
        hasKey = true;
        Debug.Log("ロボット：鍵を手に入れた！");
    }

    public void UseKey()
    {
        hasKey = false;
        Debug.Log("ロボット：鍵を開扉に使用した。");
    }

    void AnimateRobot(float timer)
    {
        bool isOffset90 = Mathf.Approximately(Mathf.Abs(modelRotationOffset), 90f);

        Quaternion rRotation = Quaternion.identity;
        Quaternion lRotation = Quaternion.identity;
        Vector3 targetBodyPos = initialBodyPosition;
        Quaternion targetBodyRot = initialBodyRotation;

        if (isJumping)
        {
            float jumpPoseSettle = Mathf.Lerp(0f, 15f, 1.0f - currentSwingAmplitude);

            if (isOffset90)
            {
                rRotation = Quaternion.Euler(0, 0, jumpPoseSettle);
                lRotation = Quaternion.Euler(0, 0, -jumpPoseSettle);
            }
            else
            {
                rRotation = Quaternion.Euler(jumpPoseSettle, 0, 0);
                lRotation = Quaternion.Euler(-jumpPoseSettle, 0, 0);
            }
            targetBodyPos = initialBodyPosition - new Vector3(0, 0.05f, 0);
        }
        else if (currentSwingAmplitude > 0.01f)
        {
            float dynamicSwingAngle = baseSwingAngle * currentSwingAmplitude;
            float wave = Mathf.Sin(timer) * dynamicSwingAngle;

            if (isOffset90)
            {
                rRotation = Quaternion.Euler(0, 0, wave);
                lRotation = Quaternion.Euler(0, 0, -wave);
            }
            else
            {
                rRotation = Quaternion.Euler(wave, 0, 0);
                lRotation = Quaternion.Euler(-wave, 0, 0);
            }

            float bob = Mathf.Abs(Mathf.Sin(timer)) * bodyBobHeight * currentSwingAmplitude;
            targetBodyPos = initialBodyPosition + new Vector3(0, bob, 0);
        }
        else
        {
            float idleBob = Mathf.Sin(Time.time * idleBreathingSpeed) * 0.02f;
            targetBodyPos = initialBodyPosition + new Vector3(0, idleBob, 0);
        }

        if (currentBodyLean != 0f)
        {
            if (isOffset90) targetBodyRot = initialBodyRotation * Quaternion.Euler(0, 0, currentBodyLean * Mathf.Sign(modelRotationOffset));
            else targetBodyRot = initialBodyRotation * Quaternion.Euler(currentBodyLean, 0, 0);
        }

        if (rArm != null) rArm.localRotation = rRotation;
        if (lLeg != null) lLeg.localRotation = rRotation;
        if (lArm != null) lArm.localRotation = lRotation;
        if (rLeg != null) rLeg.localRotation = lRotation;

        if (bodyVisual != null)
        {
            bodyVisual.localPosition = targetBodyPos;
            bodyVisual.localRotation = targetBodyRot;
        }
    }
}