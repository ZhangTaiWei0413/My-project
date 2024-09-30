using UnityEngine; 
using System.Collections;  // 引入協同程式功能

public class attackdefensesystem2p : MonoBehaviour 
{
    public bool isAttacking = false;  // 記錄玩家是否在攻擊
    public bool isDefending = false;  // 記錄玩家是否在防禦
    public bool isBeingHit = false;  // 記錄玩家是否被打中，預設為 false
    private bool isGrounded = true;  // 檢查玩家是否在地面上，預設為 true

    public int currentDefenseType = -1;  // 當前的防禦類型，預設為 -1 表示沒有防禦

    // 攻擊的前搖、中搖和後搖時間，用來控制每個階段的延遲
    public float[] attackStartupTimes = { 0.2f, 0.3f, 0.4f };  // 前搖時間陣列
    public float[] attackActiveTimes = { 0.1f, 5f, 0.2f };  // 中搖時間陣列（攻擊有效時間）
    public float[] attackRecoveryTimes = { 0.3f, 0.35f, 0.4f };  // 後搖時間陣列
    public float[] attackRanges = { 2.0f, 2.5f, 3.0f };  // 不同攻擊的範圍陣列

    public Transform player; // 玩家2的Transform，負責管理玩家的位置和旋轉
    public Transform opponent; // 玩家1的Transform，用來存玩家1的位置和旋轉
    public GameObject attackPointPrefab;  // 攻擊範圍顯示的預製體
    private GameObject activeAttackPoint; // 存放動態生成的攻擊點

    private attackdefensesystem1p opponentSystem;  // 玩家1的攻防系統，我們會用這個來檢查玩家1是否防禦成功

    void Start()  // 遊戲開始時執行
    {
        opponentSystem = opponent.GetComponent<attackdefensesystem1p>();  // 取得玩家1的攻防系統
    }

    void Update()  // 每一幀都會執行
    {
        HandleInputs();  // 檢查玩家輸入
        if (!isAttacking) UpdatePlayerFacingDirection();  // 如果沒有在攻擊，那就更新玩家的面向方向
    }

    void HandleInputs()  // 處理玩家的按鍵輸入
    {
        // 檢查是否按下了攻擊按鍵，並且現在沒有在攻擊
        if (Input.GetKeyDown(KeyCode.Keypad7) && !isAttacking)
        {
            StartCoroutine(AttackSequence(0));  // 發動高攻擊（按下數字鍵盤7）
        }
        else if (Input.GetKeyDown(KeyCode.Keypad8) && !isAttacking)
        {
            StartCoroutine(AttackSequence(1));  // 發動中攻擊（按下數字鍵盤8）
        }
        else if (Input.GetKeyDown(KeyCode.Keypad9) && !isAttacking)
        {
            StartCoroutine(AttackSequence(2));  // 發動低攻擊（按下數字鍵盤9）
        }

        // 防禦按鍵輸入
        if (Input.GetKey(KeyCode.Keypad4))
        {
            Defend(0);  // 做高防禦（按下數字鍵盤4）
            currentDefenseType = 0;  // 設定防禦類型為高防禦
        }
        else if (Input.GetKey(KeyCode.Keypad5))
        {
            Defend(1);  // 做中防禦（按下數字鍵盤5）
            currentDefenseType = 1;  // 設定防禦類型為中防禦
        }
        else if (Input.GetKey(KeyCode.Keypad6))
        {
            Defend(2);  // 做低防禦（按下數字鍵盤6）
            currentDefenseType = 2;  // 設定防禦類型為低防禦
        }
    }

    void UpdatePlayerFacingDirection()  // 更新玩家面向
    {
        // 如果玩家在對手的右邊，就讓玩家面向左，反之亦然
        if (player.position.x > opponent.position.x)
        {
            player.localScale = new Vector3(-1, 1, 1);  // 玩家面向左
        }
        else
        {
            player.localScale = new Vector3(1, 1, 1);  // 玩家面向右
        }
    }

    IEnumerator AttackSequence(int attackType)  // 這是攻擊的完整過程
    {
        isAttacking = true;  // 現在開始攻擊
        isDefending = false;  // 攻擊的時候不能防禦

        // 前搖：攻擊開始前的等待時間
        yield return new WaitForSeconds(attackStartupTimes[attackType]);

        // 根據玩家的面向方向計算攻擊點的位置
        Vector3 attackDirection = player.localScale.x > 0 ? Vector3.right : Vector3.left;  // 決定攻擊的方向
        Vector3 attackPosition = player.position + attackDirection * attackRanges[attackType];  // 計算攻擊範圍最遠點

        // 動態生成一個攻擊點
        activeAttackPoint = Instantiate(attackPointPrefab, attackPosition, Quaternion.identity);  // 生成攻擊點

        Debug.Log("玩家2 發動了 " + (attackType == 0 ? "高" : attackType == 1 ? "中" : "低") + " 攻擊");

        // 檢查對手是否在攻擊範圍內
        if (Vector2.Distance(player.position, opponent.position) <= attackRanges[attackType])  // 確認攻擊距離
        {
            if ((player.localScale.x > 0 && opponent.position.x > player.position.x) || (player.localScale.x < 0 && opponent.position.x < player.position.x))  // 確認攻擊方向
            {
                if (opponentSystem.GetCurrentDefenseType() == attackType)  // 對手的防禦類型是否匹配攻擊
                {
                    Debug.Log("玩家1 防禦成功！");
                }
                else
                {
                    Debug.Log("玩家1 防禦失敗，受到攻擊！");
                    opponentSystem.GetHit();  // 讓對手受到攻擊
                }
            }
        }

        // 中搖：攻擊有效時間
        yield return new WaitForSeconds(attackActiveTimes[attackType]);

        Destroy(activeAttackPoint);  // 攻擊結束，銷毀攻擊點

        // 後搖：攻擊恢復時間，這段時間不能做其他動作
        yield return new WaitForSeconds(attackRecoveryTimes[attackType]);
        isAttacking = false;  // 攻擊結束，恢復非攻擊狀態
    }

    void Defend(int defenseType)  // 處理防禦行為
    {
        isDefending = true;  // 現在玩家正在防禦
        isAttacking = false;  // 防禦時不能攻擊
    }

    public int GetCurrentDefenseType()  // 取得當前的防禦類型
    {
        return currentDefenseType;  // 回傳目前的防禦類型
    }

    public void GetHit()  // 處理玩家被擊中的行為
    {
        isBeingHit = true;  // 現在玩家被擊中
        StartCoroutine(WaitUntilGrounded());  // 等待玩家落地
    }

    IEnumerator WaitUntilGrounded()  // 等待玩家落地
    {
        yield return new WaitUntil(() => isGrounded);  // 等待玩家接觸地面
        yield return new WaitForSeconds(0.5f);  // 落地後再等0.5秒
        isBeingHit = false;  // 玩家不再被擊中
    }
}
