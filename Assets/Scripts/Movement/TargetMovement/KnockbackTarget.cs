using UnityEngine;
using Fragsurf.Movement;

[RequireComponent(typeof(BoxCollider))]
public class KnockbackTarget : MonoBehaviour, ISurfControllable
{
    [Header("Movement Config")]
    public MovementConfig Config = new MovementConfig();

    private SurfController _controller = new SurfController();
    private MoveData _moveData = new MoveData();
    public MoveData MoveData => _moveData;

    public MoveType MoveType { get; set; } = MoveType.Walk;
    public GameObject GroundObject { get; set; }
    public BoxCollider Collider { get; private set; }
    public Quaternion Orientation => Quaternion.identity;
    public Vector3 Forward => transform.forward;
    public Vector3 Right => transform.right;
    public Vector3 Up => transform.up;
    public Vector3 StandingExtents => Collider.bounds.extents;

    private float _elapsed;
    private float _accumulator;
    private float _alpha;

    private void Start()
    {
        Collider = GetComponent<BoxCollider>();
        Collider.isTrigger = false;

        // ✔ SurfCharacter와 동일하게 중심을 바닥 기준으로 맞춤
        Collider.center = new Vector3(0f, -0.5f, 0f);

        MoveData.Origin = transform.position;
        MoveData.GravityFactor = 1f;

        Time.fixedDeltaTime = 1f / 100f;
        Physics.autoSyncTransforms = true;
    }

    private void Update()
    {
        float now = Time.realtimeSinceStartup;
        float frameTime = Mathf.Min(now - _elapsed, Time.fixedDeltaTime);
        _elapsed = now;
        _accumulator += frameTime;

        while (_accumulator >= Time.fixedDeltaTime)
        {
            _accumulator -= Time.fixedDeltaTime;
            Tick();
        }

        _alpha = _accumulator / Time.fixedDeltaTime;

        // ✔ 위치 보간 (SurfCharacter와 일치)
        Vector3 interp = Vector3.Lerp(MoveData.PreviousOrigin, MoveData.Origin, _alpha);
        transform.position = interp;
    }

    private void Tick()
    {
        float dt = Time.fixedDeltaTime;

        // ✔ 이동 계산
        _controller.CalculateMovement(this, Config, dt);

        // ✔ 위치 이동
        MoveData.Origin += MoveData.AbsVelocity * dt;

        // ✔ 충돌 해소
        SurfPhysics.ResolveCollisions(this);

        // ✔ 지면에 닿으면 수직 속도 감쇠
        if (IsGrounded())
        {
            MoveData.Velocity.y = Mathf.Min(MoveData.Velocity.y, 0f);
        }
    }

    public void ApplyKnockback(Vector3 force)
    {
        MoveData.Velocity += force;
    }

    private bool IsGrounded()
    {
        var extents = Collider.bounds.extents * 0.99f;
        var center = MoveData.Origin + new Vector3(0, extents.y + 0.02f, 0);
        float distance = 0.1f;

        if (MoveData.Velocity.y < 0)
        {
            float dv = MoveData.Velocity.y * -1.01f * Time.fixedDeltaTime;
            distance = Mathf.Max(distance, dv);
        }

        if (Physics.BoxCast(center, extents, Vector3.down, out RaycastHit hit, Orientation, distance, SurfPhysics.GroundLayerMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.normal.y > SurfPhysics.SurfSlope)
            {
                GroundObject = hit.collider.gameObject;
                MoveData.GroundNormal = hit.normal;
                return true;
            }
        }

        GroundObject = null;
        return false;
    }
}