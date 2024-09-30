using UnityEngine; 
using System.Collections;  // �ޤJ��P�{���\��

public class attackdefensesystem2p : MonoBehaviour 
{
    public bool isAttacking = false;  // �O�����a�O�_�b����
    public bool isDefending = false;  // �O�����a�O�_�b���m
    public bool isBeingHit = false;  // �O�����a�O�_�Q�����A�w�]�� false
    private bool isGrounded = true;  // �ˬd���a�O�_�b�a���W�A�w�]�� true

    public int currentDefenseType = -1;  // ��e�����m�����A�w�]�� -1 ��ܨS�����m

    // �������e�n�B���n�M��n�ɶ��A�Ψӱ���C�Ӷ��q������
    public float[] attackStartupTimes = { 0.2f, 0.3f, 0.4f };  // �e�n�ɶ��}�C
    public float[] attackActiveTimes = { 0.1f, 5f, 0.2f };  // ���n�ɶ��}�C�]�������Įɶ��^
    public float[] attackRecoveryTimes = { 0.3f, 0.35f, 0.4f };  // ��n�ɶ��}�C
    public float[] attackRanges = { 2.0f, 2.5f, 3.0f };  // ���P�������d��}�C

    public Transform player; // ���a2��Transform�A�t�d�޲z���a����m�M����
    public Transform opponent; // ���a1��Transform�A�ΨӦs���a1����m�M����
    public GameObject attackPointPrefab;  // �����d����ܪ��w�s��
    private GameObject activeAttackPoint; // �s��ʺA�ͦ��������I

    private attackdefensesystem1p opponentSystem;  // ���a1���𨾨t�ΡA�ڭ̷|�γo�Ө��ˬd���a1�O�_���m���\

    void Start()  // �C���}�l�ɰ���
    {
        opponentSystem = opponent.GetComponent<attackdefensesystem1p>();  // ���o���a1���𨾨t��
    }

    void Update()  // �C�@�V���|����
    {
        HandleInputs();  // �ˬd���a��J
        if (!isAttacking) UpdatePlayerFacingDirection();  // �p�G�S���b�����A���N��s���a�����V��V
    }

    void HandleInputs()  // �B�z���a�������J
    {
        // �ˬd�O�_���U�F��������A�åB�{�b�S���b����
        if (Input.GetKeyDown(KeyCode.Keypad7) && !isAttacking)
        {
            StartCoroutine(AttackSequence(0));  // �o�ʰ������]���U�Ʀr��L7�^
        }
        else if (Input.GetKeyDown(KeyCode.Keypad8) && !isAttacking)
        {
            StartCoroutine(AttackSequence(1));  // �o�ʤ������]���U�Ʀr��L8�^
        }
        else if (Input.GetKeyDown(KeyCode.Keypad9) && !isAttacking)
        {
            StartCoroutine(AttackSequence(2));  // �o�ʧC�����]���U�Ʀr��L9�^
        }

        // ���m�����J
        if (Input.GetKey(KeyCode.Keypad4))
        {
            Defend(0);  // �������m�]���U�Ʀr��L4�^
            currentDefenseType = 0;  // �]�w���m�����������m
        }
        else if (Input.GetKey(KeyCode.Keypad5))
        {
            Defend(1);  // �������m�]���U�Ʀr��L5�^
            currentDefenseType = 1;  // �]�w���m�����������m
        }
        else if (Input.GetKey(KeyCode.Keypad6))
        {
            Defend(2);  // ���C���m�]���U�Ʀr��L6�^
            currentDefenseType = 2;  // �]�w���m�������C���m
        }
    }

    void UpdatePlayerFacingDirection()  // ��s���a���V
    {
        // �p�G���a�b��⪺�k��A�N�����a���V���A�Ϥ���M
        if (player.position.x > opponent.position.x)
        {
            player.localScale = new Vector3(-1, 1, 1);  // ���a���V��
        }
        else
        {
            player.localScale = new Vector3(1, 1, 1);  // ���a���V�k
        }
    }

    IEnumerator AttackSequence(int attackType)  // �o�O����������L�{
    {
        isAttacking = true;  // �{�b�}�l����
        isDefending = false;  // �������ɭԤ��ਾ�m

        // �e�n�G�����}�l�e�����ݮɶ�
        yield return new WaitForSeconds(attackStartupTimes[attackType]);

        // �ھڪ��a�����V��V�p������I����m
        Vector3 attackDirection = player.localScale.x > 0 ? Vector3.right : Vector3.left;  // �M�w��������V
        Vector3 attackPosition = player.position + attackDirection * attackRanges[attackType];  // �p������d��̻��I

        // �ʺA�ͦ��@�ӧ����I
        activeAttackPoint = Instantiate(attackPointPrefab, attackPosition, Quaternion.identity);  // �ͦ������I

        Debug.Log("���a2 �o�ʤF " + (attackType == 0 ? "��" : attackType == 1 ? "��" : "�C") + " ����");

        // �ˬd���O�_�b�����d��
        if (Vector2.Distance(player.position, opponent.position) <= attackRanges[attackType])  // �T�{�����Z��
        {
            if ((player.localScale.x > 0 && opponent.position.x > player.position.x) || (player.localScale.x < 0 && opponent.position.x < player.position.x))  // �T�{������V
            {
                if (opponentSystem.GetCurrentDefenseType() == attackType)  // ��⪺���m�����O�_�ǰt����
                {
                    Debug.Log("���a1 ���m���\�I");
                }
                else
                {
                    Debug.Log("���a1 ���m���ѡA��������I");
                    opponentSystem.GetHit();  // �����������
                }
            }
        }

        // ���n�G�������Įɶ�
        yield return new WaitForSeconds(attackActiveTimes[attackType]);

        Destroy(activeAttackPoint);  // ���������A�P�������I

        // ��n�G������_�ɶ��A�o�q�ɶ����వ��L�ʧ@
        yield return new WaitForSeconds(attackRecoveryTimes[attackType]);
        isAttacking = false;  // ���������A��_�D�������A
    }

    void Defend(int defenseType)  // �B�z���m�欰
    {
        isDefending = true;  // �{�b���a���b���m
        isAttacking = false;  // ���m�ɤ������
    }

    public int GetCurrentDefenseType()  // ���o��e�����m����
    {
        return currentDefenseType;  // �^�ǥثe�����m����
    }

    public void GetHit()  // �B�z���a�Q�������欰
    {
        isBeingHit = true;  // �{�b���a�Q����
        StartCoroutine(WaitUntilGrounded());  // ���ݪ��a���a
    }

    IEnumerator WaitUntilGrounded()  // ���ݪ��a���a
    {
        yield return new WaitUntil(() => isGrounded);  // ���ݪ��a��Ĳ�a��
        yield return new WaitForSeconds(0.5f);  // ���a��A��0.5��
        isBeingHit = false;  // ���a���A�Q����
    }
}
