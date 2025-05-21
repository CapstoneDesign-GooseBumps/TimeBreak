using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Collections.Generic;

namespace Fragsurf.Movement
{
    [AddComponentMenu("Fragsurf/Surf Character")]
    public class SurfCharacter : MonoBehaviour, ISurfControllable
    {
        [Header("Physics Settings")]
        public int TickRate = 100;
        public Vector3 ColliderSize = new Vector3(1, 1.83f, 1);

        [Header("View Settings")]
        public Camera Camera;
        public Vector3 ViewOffset = new Vector3(0, 1.64f, 0);
        public Vector3 DuckedViewOffset = new Vector3(0, 1.21f, 0);
        public int FieldOfView = 75;

        [Header("Input Settings")]
        public float XSens = 25;
        public float YSens = 25;
        public KeyCode JumpButton = KeyCode.Space;
        public KeyCode DuckButton = KeyCode.LeftControl;
        public KeyCode MoveLeft = KeyCode.A;
        public KeyCode MoveRight = KeyCode.D;
        public KeyCode MoveForward = KeyCode.W;
        public KeyCode MoveBack = KeyCode.S;
        public KeyCode Noclip = KeyCode.N;

        //public KeyCode Restart = KeyCode.T;
        public KeyCode YawLeft = KeyCode.Mouse4;
        public KeyCode YawRight = KeyCode.Mouse3;
        public int YawSpeed = 260;

        [Header("Movement Config")]
        [SerializeField]
        private MovementConfig _moveConfig = new MovementConfig();

        [Header("Trigger Events")]
        public UnityEvent<GameObject> OnTriggerEnterEvent;
        public UnityEvent<GameObject> OnTriggerStayEvent;
        public UnityEvent<GameObject> OnTriggerExitEvent;

        public TimeSkillManager timeSkill;

        private Vector3 _startPosition;
        private SurfController _controller = new SurfController();
        private List<GameObject> _touchingLastFrame = new List<GameObject>();
        private float _alpha;
        private float _accumulator;
        private float _elapsedTime;
        private bool _hasCursor = true;

        // ISurfControllable
        public MoveType MoveType { get; set; } = MoveType.Walk;
        public MovementConfig MoveConfig => _moveConfig;
        public MoveData MoveData { get; } = new MoveData();
        public BoxCollider Collider { get; private set; }
        public GameObject GroundObject { get; set; }
        public Vector3 BaseVelocity { get; }
        public Quaternion Orientation => Quaternion.identity;
        public Vector3 Forward => transform.forward;
        public Vector3 Right => transform.right;
        public Vector3 Up => transform.up;
        public Vector3 StandingExtents => ColliderSize * 0.5f;

        private void OnDestroy()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void Start()
        {
            Time.fixedDeltaTime = 1f / TickRate;
            if (Camera == null) Camera = Camera.main;
            Camera.fieldOfView = FieldOfView;
            Camera.transform.SetParent(null);

            foreach (var col in GetComponentsInChildren<Collider>()) Destroy(col);

            Collider = gameObject.AddComponent<BoxCollider>();
            Collider.size = ColliderSize;
            Collider.center = new Vector3(0, 0, 0);
            Collider.isTrigger = true;

            var rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            MoveData.Origin = transform.position;
            MoveData.ViewAngles = transform.rotation.eulerAngles;
            _moveConfig.NoclipCollide = false;
            _startPosition = transform.position;
            Physics.autoSyncTransforms = true;
        }

        private void Update()
        {
            if (_hasCursor)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            if (Input.GetKeyDown(KeyCode.Escape)) _hasCursor = !_hasCursor;

            UpdateTestBinds();
            UpdateRotation();
            UpdateMoveData();

            var now = Time.realtimeSinceStartup;
            var frameTime = Mathf.Min(now - _elapsedTime, Time.fixedDeltaTime);
            Time.maximumDeltaTime = Time.fixedDeltaTime * (1f / Time.timeScale);
            _elapsedTime = now;
            _accumulator += frameTime;

            while (_accumulator >= Time.fixedDeltaTime)
            {
                _accumulator -= Time.fixedDeltaTime;
                Tick();
            }
            _alpha = _accumulator / Time.fixedDeltaTime;

            // 3) 카메라 보간 – 시간 배수 적용 안 함
            if (Camera)
            {
                float halfHeight = Collider.size.y * transform.localScale.y * 0.5f;
                Vector3 interp = Vector3.Lerp(MoveData.PreviousOrigin, MoveData.Origin, _alpha);
                Camera.transform.position = interp
                                        + Vector3.up * halfHeight
                                        + (MoveData.Ducked ? DuckedViewOffset : ViewOffset);
            }
        }

        private void UpdateTestBinds()
        {
            /*
            if (Input.GetKeyDown(Restart))
            {
                MoveData.Velocity = Vector3.zero;
                MoveData.Origin = _startPosition;
            }
            */
            if (Input.GetKeyDown(Noclip))
                MoveType = MoveType == MoveType.Noclip ? MoveType.Walk : MoveType.Noclip;
        }
        private void Tick()
        {
            // 1) 물리 연산에만 시간 배수 적용
            float dt = Time.fixedDeltaTime * timeSkill.TimeMultiplier;
            _controller.CalculateMovement(this, _moveConfig, dt);

            // 2) 넉백 반영 (외부 힘을 직접 위치에 적용 - 감쇠 포함)
            MoveData.Origin += _externalVelocity;
            _externalVelocity *= 0.95f; // 감쇠 적용 (0.85 ~ 0.95 정도 추천)

            // 아주 작아지면 제거
            if (_externalVelocity.magnitude < 0.01f)
                _externalVelocity = Vector3.zero;

            // 3) MoveData.Origin은 발 위치 기준이므로, 중앙 기준으로 올려서 실제 위치 설정
            float halfHeight = Collider.size.y * transform.localScale.y * 0.5f;
            transform.position = MoveData.Origin + Vector3.up * halfHeight;

            // 4) 트리거 처리
            var prevTouches = new HashSet<GameObject>(_touchingLastFrame);
            var prevOrigin = MoveData.PreviousOrigin;
            var newOrigin = MoveData.Origin;
            var center = prevOrigin + Vector3.up * Collider.bounds.extents.y;
            var dir = (newOrigin - prevOrigin).normalized;
            var distance = Vector3.Distance(prevOrigin, newOrigin);

            var hits = Physics.BoxCastAll(
                center,
                Collider.bounds.extents,
                dir,
                Quaternion.identity,
                distance,
                SurfPhysics.GroundLayerMask,
                QueryTriggerInteraction.Collide
            );

            var currentTouches = hits
                .Where(h => h.collider.isTrigger)
                .Select(h => h.collider.gameObject)
                .Distinct()
                .ToList();

            var newList = currentTouches.Except(prevTouches).ToList();
            var stayList = currentTouches.Intersect(prevTouches).ToList();
            var exitList = prevTouches.Except(currentTouches).ToList();

            foreach (var go in newList)  OnTriggerEnterEvent?.Invoke(go);
            foreach (var go in stayList) OnTriggerStayEvent?.Invoke(go);
            foreach (var go in exitList)  OnTriggerExitEvent?.Invoke(go);

            _touchingLastFrame = currentTouches;
        }

        private void UpdateMoveData()
        {
            MoveData.SideMove = Input.GetKey(MoveLeft) ? -MoveConfig.Accelerate :
                Input.GetKey(MoveRight) ? MoveConfig.Accelerate : 0f;
            MoveData.ForwardMove = Input.GetKey(MoveForward) ? MoveConfig.Accelerate :
                Input.GetKey(MoveBack) ? -MoveConfig.Accelerate : 0f;

            if (Input.GetKey(JumpButton)) MoveData.Buttons |= InputActions.Jump;
            else MoveData.Buttons &= ~InputActions.Jump;
            if (Input.GetKey(DuckButton)) MoveData.Buttons |= InputActions.Duck;
            else MoveData.Buttons &= ~InputActions.Duck;

            MoveData.OldButtons = MoveData.Buttons;
            var angles = Camera.transform.rotation.eulerAngles;
            MoveData.ViewAngles = angles;
            transform.rotation = Quaternion.Euler(0, angles.y, angles.z);
        }

        private void UpdateRotation()
        {
            var angles = MoveData.ViewAngles;
            var mx = Input.GetAxis("Mouse X") * XSens * .022f;
            var my = Input.GetAxis("Mouse Y") * YSens * .022f;
            angles.x = SurfPhysics.ClampAngle(angles.x - my, -89f, 89f);
            angles.y += mx;

            var yaw = Input.GetKey(YawLeft) ? -YawSpeed :
                      Input.GetKey(YawRight) ? YawSpeed : 0;
            angles.y += yaw * Time.deltaTime;

            Camera.transform.rotation = Quaternion.Euler(angles);
            MoveData.ViewAngles = angles;
        }
        private Vector3 _externalVelocity = Vector3.zero;

        public void AddExternalVelocity(Vector3 velocity)
        {
            _externalVelocity += velocity;
        }

    }
}