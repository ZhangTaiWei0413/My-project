using UnityEngine; 
using System.Collections;  // 引入系統的集合功能，這裡用來寫協同程式 (Coroutine)

public class attackdefensesystem1p : MonoBehaviour 
{
    public bool isAttacking = false;  // 用來記錄玩家現在是不是在攻擊
    public bool isDefending = false;  // 用來記錄玩家現在是不是在防禦
    public bool isBeingHit = false;  // 用來記錄玩家有沒有被打中，初始狀態是沒有被打中
    private bool isGrounded = true;  // 用來判斷玩家有沒有站在地面上，初始值是站在地面

    public int currentDefenseType = -1;  // 這是用來記錄目前玩家在做哪種類型的防禦，-1 代表沒有在防禦

    // 這些陣列是用來記錄攻擊的前搖、中搖、後搖時間，意思就是攻擊前、攻擊時、和攻擊後要等待的時間
    public float[] attackStartupTimes = { 0.2f, 0.3f, 0.4f };  // 前搖時間，攻擊開始前的等待時間（以秒為單位）
    public float[] attackActiveTimes = { 0.1f, 5f, 0.2f };  // 中搖時間，攻擊有效的時間
    public float[] attackRecoveryTimes = { 0.3f, 0.35f, 0.4f };  // 後搖時間，攻擊結束後的恢復時間
    public float[] attackRanges = { 2.0f, 2.5f, 3.0f };  // 攻擊的距離，不同類型的攻擊有不同的範圍

    public Transform player; // 這是玩家1的Transform，負責管理玩家的位置和旋轉
    public Transform opponent; // 這是玩家2的Transform，用來存玩家2的位置和旋轉
    public GameObject attackPointPrefab;  // 這是攻擊範圍的預製物，當攻擊發動時會顯示出來
    private GameObject activeAttackPoint; // 當玩家攻擊時動態生成的攻擊點

    private attackdefensesystem2p opponentSystem;  // 玩家2的攻防系統，我們會用這個來檢查玩家2是否防禦成功

    void Start()  // 遊戲一開始就會執行這個函式
    {
        opponentSystem = opponent.GetComponent<attackdefensesystem2p>();  // 取得玩家2的攻防系統，這樣我們就能跟玩家2互動
    }

    void Update()  // 每一幀都會執行這個函式，用來更新遊戲狀態
    {
        HandleInputs();  // 檢查玩家有沒有按下攻擊或防禦的按鍵
        if (!isAttacking) UpdatePlayerFacingDirection();  // 如果沒有在攻擊，那就更新玩家的面向方向，讓玩家面向對手
    }

    void HandleInputs()  // 這裡用來處理玩家的按鍵輸入
    {
        // 檢查玩家是不是按下了攻擊鍵，並且確定現在沒有在攻擊，這樣才能開始新的攻擊
        if (Input.GetKeyDown(KeyCode.T) && !isAttacking)
        {
            StartCoroutine(AttackSequence(0));  // 發動高攻擊（按下T鍵）
        }
        else if (Input.GetKeyDown(KeyCode.Y) && !isAttacking)
        {
            StartCoroutine(AttackSequence(1));  // 發動中攻擊（按下Y鍵）
        }
        else if (Input.GetKeyDown(KeyCode.U) && !isAttacking)
        {
            StartCoroutine(AttackSequence(2));  // 發動低攻擊（按下U鍵）
        }

        // 檢查玩家是不是按下了防禦鍵
        if (Input.GetKey(KeyCode.G))
        {
            Defend(0);  // 做高防禦（按下G鍵）
            currentDefenseType = 0;  // 設定防禦類型為高防禦
        }
        else if (Input.GetKey(KeyCode.H))
        {
            Defend(1);  // 做中防禦（按下H鍵）
            currentDefenseType = 1;  // 設定防禦類型為中防禦
        }
        else if (Input.GetKey(KeyCode.J))
        {
            Defend(2);  // 做低防禦（按下J鍵）
            currentDefenseType = 2;  // 設定防禦類型為低防禦
        }
    }

    void UpdatePlayerFacingDirection()  // 這個函式用來更新玩家的面向方向
    {
        // 如果玩家1在玩家2的右邊，那就讓玩家1面向左邊，反之亦然
        if (player.position.x > opponent.position.x)
        {
            player.localScale = new Vector3(-1, 1, 1);  // 玩家面向左邊
        }
        else
        {
            player.localScale = new Vector3(1, 1, 1);  // 玩家面向右邊
        }
    }

    IEnumerator AttackSequence(int attackType)  // 這是攻擊序列，負責處理攻擊的完整過程
    {
        isAttacking = true;  // 設定玩家現在在攻擊
        isDefending = false;  // 攻擊時不能防禦，所以設置為不防禦

        // 前搖：攻擊前的等待時間
        yield return new WaitForSeconds(attackStartupTimes[attackType]);

        // 設定攻擊方向，計算攻擊範圍最遠的點
        Vector3 attackDirection = player.localScale.x > 0 ? Vector3.right : Vector3.left;  // 根據玩家的面向方向決定攻擊方向
        Vector3 attackPosition = player.position + attackDirection * attackRanges[attackType];  // 計算攻擊點的位置

        // 動態生成一個攻擊點，用來顯示攻擊範圍
        activeAttackPoint = Instantiate(attackPointPrefab, attackPosition, Quaternion.identity);  // 在攻擊位置生成攻擊點

        Debug.Log("玩家1 發動了 " + (attackType == 0 ? "高" : attackType == 1 ? "中" : "低") + " 攻擊");

        // 判斷對手是否在攻擊範圍內，並且攻擊方向是否正確
        if (Vector2.Distance(player.position, opponent.position) <= attackRanges[attackType])  // 檢查對手是否在攻擊範圍內
        {
            if ((player.localScale.x > 0 && opponent.position.x > player.position.x) || (player.localScale.x < 0 && opponent.position.x < player.position.x))  // 確定攻擊方向正確
            {
                if (opponentSystem.GetCurrentDefenseType() == attackType)  // 如果對手的防禦類型正好匹配攻擊類型
                {
                    Debug.Log("玩家2 防禦成功！");
                }
                else
                {
                    Debug.Log("玩家2 防禦失敗，受到攻擊！");
                    opponentSystem.GetHit();  // 讓對手受到攻擊
                }
            }
        }

        // 中搖：攻擊有效的時間，這段時間可以命中對手
        yield return new WaitForSeconds(attackActiveTimes[attackType]);

        Destroy(activeAttackPoint);  // 攻擊結束後銷毀攻擊點

        // 後搖：攻擊結束後的恢復時間，這段時間不能做其他動作
        yield return new WaitForSeconds(attackRecoveryTimes[attackType]);
        isAttacking = false;  // 攻擊結束，恢復到非攻擊狀態
    }

    void Defend(int defenseType)  // 這個函式用來處理防禦動作
    {
        isDefending = true;  // 設置玩家正在防禦
        isAttacking = false;  // 當防禦時，不能攻擊
    }

    public int GetCurrentDefenseType()  // 返回當前的防禦類型
    {
        return currentDefenseType;  // 返回目前的防禦類型
    }

    public void GetHit()  // 這個函式處理玩家被擊中的情況
    {
        isBeingHit = true;  // 設置玩家被打中
        StartCoroutine(WaitUntilGrounded());  // 開始等待玩家落地
    }

    IEnumerator WaitUntilGrounded()  // 等待玩家落地的協同程式
    {
        yield return new WaitUntil(() => isGrounded);  // 等到玩家碰到地面
        yield return new WaitForSeconds(0.5f);  // 落地後再等0.5秒
        isBeingHit = false;  // 玩家不再被擊中，恢復正常狀態
    }
}
