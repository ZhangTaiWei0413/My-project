using UnityEngine;
using System.Collections;

public class attackdefensesystem1p : MonoBehaviour
{
    public bool isAttacking = false;
    public bool isDefending = false;
    public bool isBeingHit = false;
    private bool isGrounded = true;

    public int currentDefenseType = -1;  // 公開以便另一個玩家能訪問

    public float attackStartupTime = 0.2f;
    public float attackActiveTime = 0.1f;
    public float attackRecoveryTime = 0.3f;
    public float attackRange = 2.0f;

    public Transform player; // 玩家1的 Transform
    public Transform opponent; // 玩家2的 Transform
    public GameObject attackRangePrefab;
    private GameObject activeAttackRange;

    private attackdefensesystem2p opponentSystem;

    void Start()
    {
        // 獲取對手的系統
        opponentSystem = opponent.GetComponent<attackdefensesystem2p>();
    }

    void Update()
    {
        HandleInputs();
        if (!isAttacking) UpdatePlayerFacingDirection();
    }

    void HandleInputs()
    {
        // 玩家1的攻擊邏輯
        if (Input.GetKeyDown(KeyCode.T) && !isAttacking)
        {
            StartCoroutine(AttackSequence(0));  // 高攻擊
        }
        else if (Input.GetKeyDown(KeyCode.Y) && !isAttacking)
        {
            StartCoroutine(AttackSequence(1));  // 中攻擊
        }
        else if (Input.GetKeyDown(KeyCode.U) && !isAttacking)
        {
            StartCoroutine(AttackSequence(2));  // 低攻擊
        }

        // 玩家1的防禦邏輯
        if (Input.GetKey(KeyCode.G))
        {
            Defend(0);  // 高防禦
            currentDefenseType = 0;
        }
        else if (Input.GetKey(KeyCode.H))
        {
            Defend(1);  // 中防禦
            currentDefenseType = 1;
        }
        else if (Input.GetKey(KeyCode.J))
        {
            Defend(2);  // 低防禦
            currentDefenseType = 2;
        }
    }

    void UpdatePlayerFacingDirection()
    {
        if (player.position.x > opponent.position.x)
        {
            player.localScale = new Vector3(-1, 1, 1); // 朝左
        }
        else
        {
            player.localScale = new Vector3(1, 1, 1);  // 朝右
        }
    }

    IEnumerator AttackSequence(int attackType)
    {
        isAttacking = true;
        isDefending = false;

        yield return new WaitForSeconds(attackStartupTime);

        // 生成攻擊範圍，設定為朝向對手方向
        Vector3 attackDirection = player.localScale.x > 0 ? Vector3.right : Vector3.left;
        activeAttackRange = Instantiate(attackRangePrefab, player.position + attackDirection * attackRange / 2, Quaternion.identity);
        activeAttackRange.transform.localScale = new Vector3(attackRange, attackRange, 1);

        Debug.Log("玩家1 發動了 " + (attackType == 0 ? "高" : attackType == 1 ? "中" : "低") + " 攻擊");

        // 判斷是否防禦成功
        if (Vector2.Distance(player.position, opponent.position) <= attackRange)
        {
            if (opponentSystem.GetCurrentDefenseType() == attackType)
            {
                Debug.Log("玩家2 防禦成功！");
            }
            else
            {
                Debug.Log("玩家2 防禦失敗，受到攻擊！");
                opponentSystem.GetHit();
            }
        }

        yield return new WaitForSeconds(attackActiveTime);

        Destroy(activeAttackRange);

        yield return new WaitForSeconds(attackRecoveryTime);
        isAttacking = false;
    }

    void Defend(int defenseType)
    {
        isDefending = true;
        isAttacking = false;
        //Debug.Log("玩家1 使用了 " + (defenseType == 0 ? "高" : defenseType == 1 ? "中" : "低") + " 防禦");
    }

    public int GetCurrentDefenseType()
    {
        return currentDefenseType;
    }

    public void GetHit()
    {
        isBeingHit = true;
        StartCoroutine(WaitUntilGrounded());
    }

    IEnumerator WaitUntilGrounded()
    {
        yield return new WaitUntil(() => isGrounded);
        yield return new WaitForSeconds(0.5f);
        isBeingHit = false;
    }
}
