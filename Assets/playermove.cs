using UnityEngine;

public class playermove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;  // 跳跃力
    public float minDistance = 1f;  // 玩家之间的最小距离
    public PhysicsMaterial2D material;  // 声明一个物理材质
    public string horizontalInput;
    public string jumpInput;      // 跳跃按键
    private Rigidbody2D rb;       // 角色的刚体
    private Collider2D playerCollider;
    private bool isGrounded;      // 用于检查是否在地面上
    public bool isTouchingPlayer;  // 用于检查是否接触到其他玩家
    private Transform otherPlayer; // 另一个玩家的Transform
    private bool disableMovement;  // 是否禁用移动
    [SerializeField] private bool isOnTopOfPlayer;  // 检查是否在敌方玩家头上,并在Inspector显示

    // 新增：用于显示当前摩擦力的值
    public float currentFriction;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();  // 获取角色的刚体
        playerCollider = GetComponent<Collider2D>();
        rb.freezeRotation = true;  // 锁定旋转

        // 根据玩家的标签设置对手玩家
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
        // 检测当前是否在接近敌方，如果是则禁用推向敌方的移动
        CheckAndDisableMovement();

        // 水平移动
        float move = Input.GetAxis(horizontalInput) * moveSpeed * Time.deltaTime;

        // 如果玩家在敌方的头上，将摩擦力设置为0，确保滑落
        if (isOnTopOfPlayer)
        {
            SetMaterialFriction(0f);  // 将摩擦力设置为0，确保滑动效果
            rb.AddForce(Vector2.down * 5f);  // 辅助向下滑动
        }

        // 如果正在推向敌方，并且已经触碰到敌方，则禁用水平移动
        if (disableMovement && IsMovingTowardsOtherPlayer(move))
        {
            move = 0;  // 禁用水平移动
        }

        // 执行水平位移（如果没有被禁用）
        if (move != 0)
        {
            transform.Translate(move, 0, 0);
        }

        // 跳跃
        if (Input.GetButtonDown(jumpInput) && isGrounded)  // 只有在地面时才可以跳跃
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            isGrounded = false;  // 跳跃后设置为空中状态
        }

        // 如果玩家不在另一玩家头上，恢复正常摩擦力
        if (!isOnTopOfPlayer && !isTouchingPlayer)
        {
            SetMaterialFriction(1f);  // 恢复正常摩擦力
        }

        // 更新当前摩擦力的显示值
        currentFriction = material.friction;
    }

    void CheckAndDisableMovement()
    {
        // 计算与敌方玩家的距离
        float distanceToOtherPlayer = Vector2.Distance(transform.position, otherPlayer.position);

        // 增加 Y 轴的容忍范围，确保滑动到一定角度时依然判断为在敌方头上
        float yTolerance = playerCollider.bounds.extents.y * 1.5f;  // 适当增加 Y 轴容忍范围
        isOnTopOfPlayer = (transform.position.y > otherPlayer.position.y - yTolerance)
                          && Mathf.Abs(transform.position.x - otherPlayer.position.x) < playerCollider.bounds.extents.x;

        // 如果两者之间的距离小于最小距离，则禁用水平移动
        disableMovement = distanceToOtherPlayer < minDistance;
    }

    // 检测当前玩家是否在向另一个玩家靠近
    bool IsMovingTowardsOtherPlayer(float move)
    {
        // 如果当前玩家在另一个玩家左侧并向右移动，或者在右侧并向左移动，则是向对方靠近
        return (transform.position.x < otherPlayer.position.x && move > 0) ||
               (transform.position.x > otherPlayer.position.x && move < 0);
    }

    // 碰撞检测：与其他玩家和地面的碰撞逻辑
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;  // 玩家接触地面
        }

        if (collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Player2"))
        {
            isTouchingPlayer = true;  // 玩家接触到其他玩家
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;  // 玩家离开地面
        }

        if (collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Player2"))
        {
            isTouchingPlayer = false;  // 玩家离开其他玩家
        }
    }

    // 设置物理材质的摩擦系数
    void SetMaterialFriction(float friction)
    {
        material.friction = friction;
        playerCollider.sharedMaterial = material;  // 应用物理材质
    }
}
