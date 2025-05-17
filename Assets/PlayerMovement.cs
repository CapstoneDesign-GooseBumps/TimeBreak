using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float normalSpeed = 5f;
    public float moveSpeed;
    public float jumpForce = 7f;
    private Rigidbody rb;
    private bool isGrounded;

    public SkillUIManager uiManager; // UI 매니저 연결

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        moveSpeed = normalSpeed;
        Time.timeScale = 1f;
    }

    void Update()
    {
        HandleSkillInput();

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(moveX, 0, moveZ) * moveSpeed;
        Vector3 newVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
        rb.linearVelocity = newVelocity;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && Time.timeScale > 0f)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void HandleSkillInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && !uiManager.IsSkillOnCooldown(0))
        {
            moveSpeed = normalSpeed * 2f;
            Time.timeScale = 2f;
            uiManager.TriggerSkill(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && !uiManager.IsSkillOnCooldown(1))
        {
            moveSpeed = normalSpeed * 0.5f;
            Time.timeScale = 0.5f;
            uiManager.TriggerSkill(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && !uiManager.IsSkillOnCooldown(2))
        {
            moveSpeed = 0f;
            Time.timeScale = 0f;
            uiManager.TriggerSkill(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && !uiManager.IsSkillOnCooldown(3))
        {
            moveSpeed = normalSpeed;
            Time.timeScale = 1f;
            uiManager.TriggerSkill(3);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}
