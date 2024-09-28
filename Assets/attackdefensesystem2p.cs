using UnityEngine;
using System.Collections;

public class attackdefensesystem2p : MonoBehaviour
{
    public bool isAttacking = false;
    public bool isDefending = false;
    public bool isBeingHit = false;
    private bool isGrounded = true;

    public int currentDefenseType = -1;  // ���}�H�K�t�@�Ӫ��a��X��

    public float attackStartupTime = 0.2f;
    public float attackActiveTime = 0.1f;
    public float attackRecoveryTime = 0.3f;
    public float attackRange = 2.0f;

    public Transform player; // ���a2�� Transform
    public Transform opponent; // ���a1�� Transform
    public GameObject attackRangePrefab;
    private GameObject activeAttackRange;

    private attackdefensesystem1p opponentSystem;

    void Start()
    {
        // �����⪺�t��
        opponentSystem = opponent.GetComponent<attackdefensesystem1p>();
    }

    void Update()
    {
        HandleInputs();
        if (!isAttacking) UpdatePlayerFacingDirection();
    }

    void HandleInputs()
    {
        // ���a2�������޿�
        if (Input.GetKeyDown(KeyCode.Keypad7) && !isAttacking)
        {
            StartCoroutine(AttackSequence(0));  // ������
        }
        else if (Input.GetKeyDown(KeyCode.Keypad8) && !isAttacking)
        {
            StartCoroutine(AttackSequence(1));  // ������
        }
        else if (Input.GetKeyDown(KeyCode.Keypad9) && !isAttacking)
        {
            StartCoroutine(AttackSequence(2));  // �C����
        }

        // ���a2�����m�޿�
        if (Input.GetKey(KeyCode.Keypad4))
        {
            Defend(0);  // �����m
            currentDefenseType = 0;
        }
        else if (Input.GetKey(KeyCode.Keypad5))
        {
            Defend(1);  // �����m
            currentDefenseType = 1;
        }
        else if (Input.GetKey(KeyCode.Keypad6))
        {
            Defend(2);  // �C���m
            currentDefenseType = 2;
        }
    }

    void UpdatePlayerFacingDirection()
    {
        if (player.position.x > opponent.position.x)
        {
            player.localScale = new Vector3(-1, 1, 1); // �¥�
        }
        else
        {
            player.localScale = new Vector3(1, 1, 1);  // �¥k
        }
    }

    IEnumerator AttackSequence(int attackType)
    {
        isAttacking = true;
        isDefending = false;

        yield return new WaitForSeconds(attackStartupTime);

        // �ͦ������d��A�]�w���¦V����V
        Vector3 attackDirection = player.localScale.x > 0 ? Vector3.right : Vector3.left;
        activeAttackRange = Instantiate(attackRangePrefab, player.position + attackDirection * attackRange / 2, Quaternion.identity);
        activeAttackRange.transform.localScale = new Vector3(attackRange, attackRange, 1);

        Debug.Log("���a2 �o�ʤF " + (attackType == 0 ? "��" : attackType == 1 ? "��" : "�C") + " ����");

        // �P�_�O�_���m���\
        if (Vector2.Distance(player.position, opponent.position) <= attackRange)
        {
            if (opponentSystem.GetCurrentDefenseType() == attackType)
            {
                Debug.Log("���a1 ���m���\�I");
            }
            else
            {
                Debug.Log("���a1 ���m���ѡA��������I");
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
        //Debug.Log("���a2 �ϥΤF " + (defenseType == 0 ? "��" : defenseType == 1 ? "��" : "�C") + " ���m");
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
