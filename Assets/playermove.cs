using UnityEngine;

public class playermove : MonoBehaviour
{
    public float moveSpeed;
    public float jumpForce;  // 跳躍力
    public float minDistance = 1f;  // 玩家之間的最小距離
    public PhysicsMaterial2D material;  // 聲明一個物理材質
    public string horizontalInput;
    public string jumpInput;      // 跳躍按鍵
    private Rigidbody2D rb;       // 角色的剛體
    private Collider2D playerCollider;
    private bool isGrounded;      // 用於檢查是否在地面上
    public bool isTouchingPlayer;  // 用於檢查是否接觸到其他玩家
    private Transform otherPlayer; // 另一個玩家的Transform
    private bool disableMovement;  // 是否禁用移動
    [SerializeField] private bool isOnTopOfPlayer;  // 檢查是否在敵方玩家頭上，並在Inspector顯示

    // 新增：用於顯示當前摩擦力的值
    public float currentFriction;

    void Start()
    {
        moveSpeed = 10f;
        jumpForce = 7f;  // 跳躍力
        rb = GetComponent<Rigidbody2D>();  // 獲取角色的剛體
        playerCollider = GetComponent<Collider2D>();
        rb.freezeRotation = true;  // 鎖定旋轉

        // 根據玩家的標籤設定對手玩家
        if (gameObject.CompareTag("Player1"))
        {
            otherPlayer = GameObject.FindGameObjectWithTag("Player2").transform;
        }
        else if (gameObject.CompareTag("Player2"))
        {
            otherPlayer = GameObject.FindGameObjectWithTag("Player1").transform;
        }
    }

    void Update()
    {
        // 檢測當前是否在接近敵方，如果是則禁用推向敵方的移動
        CheckAndDisableMovement();

        // 水平移動
        float move = Input.GetAxis(horizontalInput) * moveSpeed * Time.deltaTime;

        // 如果玩家在敵方的頭上，將摩擦力設置為0，確保滑落
        if (isOnTopOfPlayer)
        {
            SetMaterialFriction(0f);  // 將摩擦力設置為0，確保滑動效果
            rb.AddForce(Vector2.down * 5f);  // 輔助向下滑動
        }

        // 如果正在推向敵方，並且已經觸碰到敵方，則禁用水平移動
        if (disableMovement && IsMovingTowardsOtherPlayer(move))
        {
            move = 0;  // 禁用水平移動
        }

        // 執行水平位移（如果沒有被禁用）
        if (move != 0)
        {
            transform.Translate(move, 0, 0);
        }

        // 跳躍
        if (Input.GetButtonDown(jumpInput) && isGrounded)  // 只有在地面時才可以跳躍
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            isGrounded = false;  // 跳躍後設置為空中狀態
        }

        // 如果玩家不在另一玩家頭上，恢復正常摩擦力
        if (!isOnTopOfPlayer && !isTouchingPlayer)
        {
            SetMaterialFriction(50f);  // 恢復正常摩擦力
        }

        // 更新當前摩擦力的顯示值
        currentFriction = material.friction;
    }

    void CheckAndDisableMovement()
    {
        // 計算與敵方玩家的距離
        float distanceToOtherPlayer = Vector2.Distance(transform.position, otherPlayer.position);

        // 當前玩家的 Y 軸位置比對手高時，才進行站在頭上的判定
        if (transform.position.y > otherPlayer.position.y)
        {
            // 增加 Y 軸的容忍範圍，確保滑動到一定角度時依然判斷為在敵方頭上
            float yTolerance = playerCollider.bounds.extents.y * 2f;  // 增加 Y 軸容忍範圍
            float xTolerance = playerCollider.bounds.extents.x * 1.5f;  // 增加 X 軸的容忍範圍

            // 判定是否在對方玩家的頭上
            isOnTopOfPlayer = (transform.position.y > otherPlayer.position.y - yTolerance)
                              && Mathf.Abs(transform.position.x - otherPlayer.position.x) < xTolerance;
        }
        else
        {
            isOnTopOfPlayer = false;  // 如果當前玩家沒有高於對手，則不觸發
        }

        // 如果兩者之間的距離小於最小距離，則禁用水平移動
        disableMovement = distanceToOtherPlayer < minDistance;
    }

    // 檢測當前玩家是否在向另一個玩家靠近
    bool IsMovingTowardsOtherPlayer(float move)
    {
        // 如果當前玩家在另一個玩家左側並向右移動，或者在右側並向左移動，則是向對方靠近
        return (transform.position.x < otherPlayer.position.x && move > 0) ||
               (transform.position.x > otherPlayer.position.x && move < 0);
    }

    // 碰撞檢測：與其他玩家和地面的碰撞邏輯
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;  // 玩家接觸地面
        }

        if (collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Player2"))
        {
            isTouchingPlayer = true;  // 玩家接觸到其他玩家
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;  // 玩家離開地面
        }

        if (collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Player2"))
        {
            isTouchingPlayer = false;  // 玩家離開其他玩家
        }
    }

    // 設置物理材質的摩擦係數
    void SetMaterialFriction(float friction)
    {
        material.friction = friction;
        playerCollider.sharedMaterial = material;  // 應用物理材質
    }
}
