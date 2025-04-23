using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SourceLikeMovement : MonoBehaviour {

    enum MoveType { Ground, Air, Surf }

    [Header("Movement Settings")]
    public float groundMaxSpeed = 6.35f;              // 250 HU/s
    public float airMaxSpeed = 6.35f;                 // 최대 공중 속도 (Ground과 동일하거나 별도 설정)
    public float gravity = 20.32f;
    public float jumpPower = 7.15f;
    public float accelerate = 10f;
    public float airAccelerate = 38.1f;
    public float friction = 4f;

    [Header("Surf Settings")]
    public float surfSlopeThreshold = 0.7f;

    [Header("Duck Settings")]
    public float standingHeight = 1.8f;
    public float duckedHeight = 1.0f;
    public float duckSpeedModifier = 0.4f;

    private CharacterController controller;
    private Camera cam;
    private Vector3 velocity;
    private MoveType moveType = MoveType.Ground;
    private bool isDucked = false;

    void Start() {
        controller = GetComponent<CharacterController>();
        cam = Camera.main;
        controller.height = standingHeight;
    }

    void Update() {
        float deltaTime = Time.deltaTime;

        Vector3 inputDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        inputDir = (camForward.normalized * inputDir.z + camRight.normalized * inputDir.x).normalized;

        // Space or mouse wheel triggers jump
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        bool scrollJump = scroll != 0;
        bool isJumping = Input.GetButtonDown("Jump") || scrollJump;
        bool duckHeld = Input.GetKey(KeyCode.LeftControl);
        UpdateDuckState(duckHeld);

        UpdateMoveType();

        if (moveType == MoveType.Ground)
        {
            ApplyFriction();
            float speed = isDucked ? groundMaxSpeed * duckSpeedModifier : groundMaxSpeed;
            Accelerate(inputDir, speed, accelerate, deltaTime);

            if (isJumping)
            {
                velocity.y = jumpPower;
                moveType = MoveType.Air;
            }
        }
        else
        {
            ApplyAirAccel(inputDir, airAccelerate, deltaTime);
            velocity.y -= gravity * deltaTime;
        }

        controller.Move(velocity * deltaTime);
    }

    void UpdateMoveType()
    {
        if (controller.isGrounded)
        {
            if (IsSurfSurface(out RaycastHit hit))
                moveType = MoveType.Surf;
            else
            {
                moveType = MoveType.Ground;
                velocity.y = -1f;
            }
        }
        else
        {
            moveType = MoveType.Air;
        }
    }

    bool IsSurfSurface(out RaycastHit hit)
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        if (Physics.Raycast(origin, Vector3.down, out hit, 1.2f))
            return hit.normal.y < surfSlopeThreshold;
        return false;
    }

    void Accelerate(Vector3 wishDir, float wishSpeed, float accel, float deltaTime)
    {
        float currentSpeed = Vector3.Dot(velocity, wishDir);
        float addSpeed = wishSpeed - currentSpeed;
        if (addSpeed <= 0) return;

        float accelSpeed = accel * wishSpeed * deltaTime;
        accelSpeed = Mathf.Min(accelSpeed, addSpeed);
        velocity += accelSpeed * wishDir;
    }

    void ApplyAirAccel(Vector3 wishDir, float accel, float deltaTime)
    {
        Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z);
        float currentSpeed = Vector3.Dot(horizontalVel, wishDir);
        float wishSpeed = airMaxSpeed;
        float addSpeed = wishSpeed - currentSpeed;
        if (addSpeed <= 0 || wishDir == Vector3.zero) return;

        float accelSpeed = accel * wishSpeed * deltaTime;
        accelSpeed = Mathf.Min(accelSpeed, addSpeed);

        horizontalVel += accelSpeed * wishDir;
        velocity.x = horizontalVel.x;
        velocity.z = horizontalVel.z;
    }

    void ApplyFriction()
    {
        Vector3 horizontal = new Vector3(velocity.x, 0, velocity.z);
        float speed = horizontal.magnitude;
        if (speed < 0.1f) return;

        float drop = speed * friction * Time.deltaTime;
        float newSpeed = Mathf.Max(speed - drop, 0);
        horizontal *= newSpeed / speed;

        velocity.x = horizontal.x;
        velocity.z = horizontal.z;
    }

    void UpdateDuckState(bool duckHeld)
    {
        if (duckHeld && !isDucked)
        {
            controller.height = duckedHeight;
            isDucked = true;
        }
        else if (!duckHeld && isDucked)
        {
            controller.height = standingHeight;
            isDucked = false;
        }
    }
}