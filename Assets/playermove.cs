using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playermove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;  // ���D�O
    public string horizontalInput;
    public string jumpInput;      // ���D����
    private Rigidbody2D rb;       // ���⪺����
    private bool isGrounded;      // �Ω��ˬd�O�_�b�a���W

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();  // ������⪺����
    }

    void Update()
    {
        // ��������
        float move = Input.GetAxis(horizontalInput) * moveSpeed * Time.deltaTime;
        transform.Translate(move, 0, 0);

        // ���D
        if (Input.GetButtonDown(jumpInput) && isGrounded)  // �u���b�a���ɤ~�i�H���D
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            isGrounded = false;  // ���D��]�m���Ť����A
        }
    }

    // �ˬd�O�_��Ĳ�a��
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;  // ��Ĳ�a���ɤ��\���D
        }
    }
}
