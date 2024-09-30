using UnityEngine;  // 引入Unity的命名空間，用於使用Unity相關的類別和方法

public class playermove : MonoBehaviour  // 定義一個繼承自MonoBehaviour的類別，MonoBehaviour是所有Unity腳本的基礎類
{
    public float moveSpeed;  // 用於控制玩家的水平移動速度
    public float jumpForce;  // 用於控制玩家跳躍時的力量
    public float minDistance = 1f;  // 玩家與另一玩家之間的最小距離，當兩者距離小於此數值時，禁用推動移動
    public PhysicsMaterial2D material;  // 物理材質，用於控制玩家與其他物體之間的摩擦力
    public string horizontalInput;  // 儲存水平移動按鍵名稱
    public string jumpInput;  // 儲存跳躍按鍵名稱
    private Rigidbody2D rb;  // 剛體元件，控制物理運動，並應用力和速度等
    private Collider2D playerCollider;  // 碰撞器元件，用來檢測碰撞
    private bool isGrounded;  // 用來檢查玩家是否接觸到地面
    public bool isTouchingPlayer;  // 用來檢查玩家是否接觸到其他玩家
    private Transform otherPlayer;  // 用來儲存另一玩家的Transform，用於計算距離
    private bool disableMovement;  // 控制是否禁用玩家移動，當兩玩家太靠近時禁用
    [SerializeField] private bool isOnTopOfPlayer;  // 用來檢查玩家是否站在另一玩家的頭上，並顯示在Inspector中

    public float currentFriction;  // 用來顯示當前摩擦力的值，供調試使用

    // Start方法會在遊戲開始時自動執行
    void Start()
    {
        moveSpeed = 10f;  // 初始化玩家的水平移動速度為10
        jumpForce = 7f;  // 初始化跳躍力為7
        rb = GetComponent<Rigidbody2D>();  // 獲取附加在此物件上的剛體元件
        playerCollider = GetComponent<Collider2D>();  // 獲取附加在此物件上的碰撞器元件
        rb.freezeRotation = true;  // 鎖定剛體的旋轉，防止角色因物理影響而翻轉

        // 根據玩家的標籤判斷對手玩家，並將其Transform存入otherPlayer變數
        if (gameObject.CompareTag("Player1"))  // 如果此物件的標籤是"Player1"
        {
            otherPlayer = GameObject.FindGameObjectWithTag("Player2").transform;  // 查找並儲存標籤為"Player2"的物件的Transform
        }
        else if (gameObject.CompareTag("Player2"))  // 如果此物件的標籤是"Player2"
        {
            otherPlayer = GameObject.FindGameObjectWithTag("Player1").transform;  // 查找並儲存標籤為"Player1"的物件的Transform
        }
    }

    // Update方法會在每一幀調用，適合處理連續變化的行為
    void Update()
    {
        CheckAndDisableMovement();  // 檢查並根據條件禁用玩家的移動

        float move = Input.GetAxis(horizontalInput) * moveSpeed * Time.deltaTime;  // 根據水平輸入和速度計算水平移動量

        if (isOnTopOfPlayer)  // 如果玩家在另一個玩家的頭上
        {
            SetMaterialFriction(0f);  // 設置摩擦力為0，確保滑動
            rb.AddForce(Vector2.down * 5f);  // 向下施加一個額外的力，幫助玩家從另一玩家頭上下滑
        }

        if (disableMovement && IsMovingTowardsOtherPlayer(move))  // 如果移動被禁用，且玩家正朝另一玩家移動
        {
            move = 0;  // 禁止水平移動
        }

        if (move != 0)  // 如果允許移動
        {
            transform.Translate(move, 0, 0);  // 執行水平移動
        }

        if (Input.GetButtonDown(jumpInput) && isGrounded)  // 如果按下跳躍鍵，且玩家在地面上
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);  // 向上施加一個瞬間的力，讓玩家跳起來
            isGrounded = false;  // 設置為不在地面上
        }

        if (!isOnTopOfPlayer && !isTouchingPlayer)  // 如果玩家不在另一個玩家的頭上，也沒有接觸其他玩家
        {
            SetMaterialFriction(50f);  // 恢復正常的摩擦力
        }

        currentFriction = material.friction;  // 更新當前的摩擦力數值，供調試和觀察
    }

    // 檢查並禁用玩家的移動
    void CheckAndDisableMovement()
    {
        float distanceToOtherPlayer = Vector2.Distance(transform.position, otherPlayer.position);  // 計算與另一玩家的距離

        if (transform.position.y > otherPlayer.position.y)  // 如果當前玩家的Y軸位置高於另一玩家
        {
            float yTolerance = playerCollider.bounds.extents.y * 2f;  // 計算Y軸上的容忍範圍
            float xTolerance = playerCollider.bounds.extents.x * 1.5f;  // 計算X軸上的容忍範圍

            isOnTopOfPlayer = (transform.position.y > otherPlayer.position.y - yTolerance)  // 檢查是否在對方頭上
                              && Mathf.Abs(transform.position.x - otherPlayer.position.x) < xTolerance;
        }
        else  // 如果當前玩家的Y軸位置低於或等於另一玩家
        {
            isOnTopOfPlayer = false;  // 設置為不在對方頭上
        }

        disableMovement = distanceToOtherPlayer < minDistance;  // 如果兩者之間的距離小於最小距離，禁用移動
    }

    // 判斷玩家是否在朝另一個玩家移動
    bool IsMovingTowardsOtherPlayer(float move)
    {
        return (transform.position.x < otherPlayer.position.x && move > 0) ||  // 如果當前玩家在左邊並向右移動
               (transform.position.x > otherPlayer.position.x && move < 0);  // 或當前玩家在右邊並向左移動
    }

    // 碰撞事件：當角色與其他物件碰撞時調用
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))  // 如果碰到地面
        {
            isGrounded = true;  // 設置玩家為站在地面上
        }

        if (collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Player2"))  // 如果碰到另一個玩家
        {
            isTouchingPlayer = true;  // 設置玩家為接觸另一玩家
        }
    }

    // 當角色離開其他物件時調用
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))  // 如果離開地面
        {
            isGrounded = false;  // 設置玩家為離開地面
        }

        if (collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Player2"))  // 如果離開另一玩家
        {
            isTouchingPlayer = false;  // 設置玩家為離開另一玩家
        }
    }

    // 設置物理材質的摩擦係數
    void SetMaterialFriction(float friction)
    {
        material.friction = friction;  // 設置摩擦力
        playerCollider.sharedMaterial = material;  // 將更新後的物理材質應用到碰撞器上
    }
}
