using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playermove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;  // 跳躍力
    public string horizontalInput;
    public string jumpInput;      // 跳躍按鍵
    private Rigidbody2D rb;       // 角色的剛體
    private bool isGrounded;      // 用於檢查是否在地面上

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();  // 獲取角色的剛體
    }

    void Update()
    {
        // 水平移動
        float move = Input.GetAxis(horizontalInput) * moveSpeed * Time.deltaTime;
        transform.Translate(move, 0, 0);

        // 跳躍
        if (Input.GetButtonDown(jumpInput) && isGrounded)  // 只有在地面時才可以跳躍
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            isGrounded = false;  // 跳躍後設置為空中狀態
        }
    }

    // 檢查是否接觸地面
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;  // 接觸地面時允許跳躍
        }
    }
}
