using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-02-21 PM 8:23:22
// 작성자 : Rito

namespace Rito.FpsTpsCharacter
{
    public class PhysicsBasedMovement : MonoBehaviour
    {
        /***********************************************************************
        *                               Definitions
        ***********************************************************************/
        #region .
        [Serializable]
        public class Components
        {
            [HideInInspector] public CapsuleCollider capsule;
            [HideInInspector] public Rigidbody rBody;

            [HideInInspector] public CapsuleCollider subCapsule;
            [HideInInspector] public Rigidbody subRBody;
        }
        [Serializable]
        public class CheckOption
        {
            [Tooltip("지면으로 체크할 레이어 설정")]
            public LayerMask groundLayerMask = -1;

            [Range(0.1f, 10.0f), Tooltip("지면 감지 거리")]
            public float groundCheckDistance = 2.0f;

            [Range(0.01f, 0.3f), Tooltip("전방 감지 거리")]
            public float forwardCheckDistance = 0.1f;

            [Range(15f, 70f), Tooltip("등반 가능한 경사각")]
            public float maxSlopeAngle = 50f;

            [Range(1f, 3f), Tooltip("경사로 이동속도 변화율(가속/감속)")]
            public float slopeSpeedChangeRate = 2f;
        }
        [Serializable]
        public class MovementOption
        {
            [Range(1f, 10f), Tooltip("이동속도")]
            public float speed = 5f;

            [Range(1f, 3f), Tooltip("달리기 이동속도 증가 계수")]
            public float runningCoef = 1.5f;

            [Range(1f, 10f), Tooltip("점프 강도")]
            public float jumpForce = 5.5f;

            [Range(0.0f, 2.0f), Tooltip("점프 쿨타임")]
            public float jumpCooldown = 0.6f;

            [Range(0, 3), Tooltip("점프 허용 횟수")]
            public int maxJumpCount = 1;
        }
        [Serializable]
        public class CurrentState
        {
            public bool isMoving;
            public bool isRunning;
            public bool isGrounded;
            public bool isOnSteepSlope; // 등반 불가능한 경사로에 올라와 있음
            public bool isJumpTriggered;
            public bool isJumping;
            public bool isForwardBlocked;
        }
        [Serializable]
        public class CurrentValue
        {
            public Vector3 worldMoveDir;
            public Vector3 groundNormal;
            public Vector3 groundCross;
            public Vector3 finalVelocity;

            [Space]
            public float jumpCooldown;
            public int jumpCount;

            [Space]
            public float groundDistance;
            public float groundSlopeAngle;         // 현재 바닥의 경사각
            public float groundVerticalSlopeAngle; // 수직으로 재측정한 경사각
            public float forwardSlopeAngle; // 캐릭터가 바라보는 방향의 경사각

            [Space]
            public float gravity; // 직접 제어하는 중력값
        }

        #endregion
        /***********************************************************************
        *                               Variables
        ***********************************************************************/
        #region .

        [SerializeField] private Components _components = new Components();
        [SerializeField] private CheckOption _checkOptions = new CheckOption();
        [SerializeField] private MovementOption _moveOptions = new MovementOption();
        [SerializeField] private CurrentState _currentStates = new CurrentState();
        [SerializeField] private CurrentValue _currentValues = new CurrentValue();

        public Components Com => _components;
        public CheckOption COption => _checkOptions;
        public MovementOption MOption => _moveOptions;
        public CurrentState State => _currentStates;
        public CurrentValue Current => _currentValues;


        private float _capsuleHeightDiff;

        private float _fixedDeltaTime;

        #endregion
        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .
        private void Awake()
        {
            InitComponents();
            InitSubComponents();
        }

        private void FixedUpdate()
        {
            _fixedDeltaTime = Time.fixedDeltaTime;

            CheckGroundSweepTest();
            CheckForwardSweepTest();

            UpdatePhysics();
            Move();
        }

        #endregion
        /***********************************************************************
        *                               Init Methods
        ***********************************************************************/
        #region .
        private void InitComponents()
        {
            TryGetComponent(out Com.rBody);
            TryGetComponent(out Com.capsule);

            // 회전은 트랜스폼을 통해 직접 제어할 것이기 때문에 리지드바디 회전은 제한
            Com.rBody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        private void InitSubComponents()
        {
            GameObject subObject = new GameObject("SUB");
            subObject.transform.SetParent(transform);
            subObject.transform.localPosition = default;

            Com.subCapsule = subObject.AddComponent<CapsuleCollider>();
            Com.subRBody = subObject.AddComponent<Rigidbody>();

            Com.subCapsule.direction = Com.capsule.direction;
            Com.subCapsule.center = Com.capsule.center;
            Com.subCapsule.height = Com.capsule.height * 0.95f;
            Com.subCapsule.radius = Com.capsule.radius * 0.95f;
            Com.subCapsule.isTrigger = true;

            Com.subRBody.useGravity = false;
            Com.subRBody.constraints = RigidbodyConstraints.FreezeAll;

            // 캡슐 높이간 차이
            _capsuleHeightDiff = (Com.capsule.height - Com.subCapsule.height) * 0.5f + 0.02f;
        }

        #endregion
        /***********************************************************************
        *                               Public Methods
        ***********************************************************************/
        #region .
        public void SetMovement(in Vector3 worldMoveDir, bool isRunning)
        {
            Current.worldMoveDir = worldMoveDir;
            State.isMoving = worldMoveDir.sqrMagnitude > 0.01f;
            State.isRunning = isRunning;
        }
        public bool SetJump()
        {
            //if(!State.isGrounded) return false;
            if(Current.jumpCooldown > 0f) return false;
            if(Current.jumpCount >= MOption.maxJumpCount) return false;

            State.isJumpTriggered = true;
            return true;
        }

        public bool IsGrounded() => State.isGrounded;

        public float GetDistanceFromGround() => Current.groundDistance;

        #endregion
        /***********************************************************************
        *                               Private Methods
        ***********************************************************************/
        #region .

        private void CheckGroundSweepTest()
        {
            Current.groundDistance = float.MaxValue;
            Current.groundNormal = Vector3.up;
            Current.groundSlopeAngle = 0f;
            Current.forwardSlopeAngle = 0f;

            bool sweep =
                Com.subRBody.SweepTest(Vector3.down, out var hitD, COption.groundCheckDistance, QueryTriggerInteraction.Ignore);

            State.isGrounded = false;

            if (sweep)
            {
                // 뚝 끊기는 지형 부드럽게 이동하도록 구현 :
                // 이동방향 전방으로 체크해서 이중 스윕테스트
                Vector3 fwDownSweepDir = (Current.worldMoveDir + Vector3.down * 0.5f).normalized;

                bool sweepFD =
                    Com.subRBody.SweepTest(fwDownSweepDir, out var hitFD, COption.groundCheckDistance, QueryTriggerInteraction.Ignore);

                // 지면 노멀벡터 초기화
                Current.groundNormal = sweepFD ? (hitD.normal + hitFD.normal) * 0.5f : hitD.normal;

                _gzForwardGroundTouch = sweepFD ? hitFD.point : Vector3.down * 999f;

                // 현재 위치한 지면의 경사각 구하기(캐릭터 이동방향 고려)
                Current.groundSlopeAngle = Vector3.Angle(Current.groundNormal, Vector3.up);
                Current.forwardSlopeAngle = Vector3.Angle(Current.groundNormal, Current.worldMoveDir) - 90f;

                State.isOnSteepSlope = Current.groundSlopeAngle >= COption.maxSlopeAngle;

                // 경사각 이중검증 (수직 레이캐스트) : 뾰족하거나 각진 부분 체크
                if (State.isOnSteepSlope)
                {
                    Vector3 ro = hitD.point + Vector3.up * 0.1f;
                    Vector3 rd = Vector3.down;
                    bool rayD = 
                        Physics.Raycast(ro, rd, out var hitRayD, 0.2f, COption.groundLayerMask, QueryTriggerInteraction.Ignore);

                    Current.groundVerticalSlopeAngle = rayD ? Vector3.Angle(hitRayD.normal, Vector3.up) : Current.groundSlopeAngle;

                    State.isOnSteepSlope = Current.groundVerticalSlopeAngle >= COption.maxSlopeAngle;
                }

                State.isGrounded = 
                    (hitD.distance < _capsuleHeightDiff) && !State.isOnSteepSlope;

                _gzGroundTouch = hitD.point;

                Current.groundDistance = transform.position.y - hitD.point.y;
            }

            // 월드 이동벡터 회전축
            Current.groundCross = Vector3.Cross(Current.groundNormal, Vector3.up);
        }

        private void CheckForwardSweepTest()
        {
            bool sweep =
                Com.subRBody.SweepTest(Current.worldMoveDir + Vector3.down * 0.2f,
                out var hit, COption.forwardCheckDistance, QueryTriggerInteraction.Ignore);

            State.isForwardBlocked = false;
            if (sweep)
            {
                _gzForwardTouch = hit.point;

                float forwardObstacleAngle = Vector3.Angle(hit.normal, Vector3.up);

                State.isForwardBlocked = forwardObstacleAngle >= COption.maxSlopeAngle;
            }
        }

        private void UpdatePhysics()
        {
            // Custom Gravity, Jumping State, Using Gravity
            if (State.isGrounded)
            {
                Current.gravity = 0f;
                Com.rBody.useGravity = false;

                State.isJumping = false;
                Current.jumpCount = 0;
            }
            else
            {
                Current.gravity += _fixedDeltaTime * Physics.gravity.y;
                Com.rBody.useGravity = true;
            }

            // Calculate Jump Cooldown
            if (Current.jumpCooldown > 0f)
                Current.jumpCooldown -= _fixedDeltaTime;
        }

        private void Move()
        {
            // 0. 가파른 경사면에 있는 경우 : 꼼짝말고 미끄럼틀 타기
            if (State.isOnSteepSlope && Current.groundDistance < 0.1f)
            {
                DebugMark(0);

                Current.finalVelocity = Vector3.up * Current.gravity;

                _gzSlideDir =
                    Quaternion.AngleAxis(90f - Current.groundSlopeAngle, Current.groundCross) * Vector3.down;

                Current.finalVelocity =
                    Quaternion.AngleAxis(90f - Current.groundSlopeAngle, Current.groundCross) * Current.finalVelocity;

                Com.rBody.velocity = Current.finalVelocity;

                return;
            }

            // 1. XZ 이동속도 계산
            if (State.isForwardBlocked && !State.isGrounded) // 공중에서 전방이 막힌 경우
            {
                DebugMark(1);
                Current.finalVelocity =
                    //Mathf.Min(0f, Current.finalVelocity.y) * Vector3.up;
                    Vector3.zero;
            }
            else // 이동 가능한 경우 : 지상 or 전방이 막히지 않음
            {
                DebugMark(2);
                float speed = 0f;
                if (State.isMoving)
                {
                    speed = MOption.speed;
                    if (State.isRunning)
                        speed *= MOption.runningCoef;
                }

                Current.finalVelocity = Current.worldMoveDir * speed;
            }

            // 2. 중력 합산
            Current.finalVelocity.y += Current.gravity;

            // 3. 벡터 회전 및 최종 속도 계산
            if (State.isGrounded || Current.groundDistance < COption.groundCheckDistance && !State.isJumping) // 지상
            {
                DebugMark(3);

                if (State.isMoving && !State.isForwardBlocked)
                {
                    DebugMark(4);

                    // 벡터 회전 (경사로)
                    Current.finalVelocity =
                        Quaternion.AngleAxis(-Current.groundSlopeAngle, Current.groundCross) * Current.finalVelocity;

                    _gzRotatedWorldMoveDir =
                        Quaternion.AngleAxis(-Current.groundSlopeAngle, Current.groundCross) * Current.worldMoveDir;
                }

                // 최종 속도 계산
                Com.rBody.velocity = Current.finalVelocity;
            }
            else // 공중
            {
                Com.rBody.velocity =
                    new Vector3(Current.finalVelocity.x, Com.rBody.velocity.y, Current.finalVelocity.z);
            }

            // 4. 점프
            if (State.isJumpTriggered && Current.jumpCooldown <= 0f)
            {
                State.isJumping = true;

                // 하강 중 점프 시 속도가 합산되지 않도록 속도 초기화
                Com.rBody.velocity = Vector3.zero;
                Com.rBody.AddForce(Vector3.up * MOption.jumpForce, ForceMode.VelocityChange);

                // 점프 쿨타임, 트리거 초기화
                Current.jumpCooldown = MOption.jumpCooldown;
                State.isJumpTriggered = false;

                // 커스텀 중력 초기화
                Current.gravity = 0f;

                Current.jumpCount++;
            }
        }

        #endregion
        /***********************************************************************
        *                               Debugs
        ***********************************************************************/
        #region .

#if UNITY_EDITOR
        public bool _debugOn;
        public int _debugIndex;
#endif

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void DebugMark(int index)
        {
            if(!_debugOn) return;
            Debug.Log("MARK - " + index);
            _debugIndex = index;
        }

        #endregion
        /***********************************************************************
        *                               Gizmos
        ***********************************************************************/
        #region .
        private Vector3 _gzGroundTouch;
        private Vector3 _gzForwardGroundTouch;
        private Vector3 _gzForwardTouch;
        private Vector3 _gzRotatedWorldMoveDir;
        
        private Vector3 _gzSlideDir; // 가파른 경사면에서 미끄럼틀 탈 방향

        [Header("Gizmos Option"), SerializeField, Range(0.1f, 2f)]
        private float _gizmoRadius = 0.1f;

        private void OnDrawGizmos()
        {
            if (Application.isPlaying == false) return;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_gzGroundTouch, _gizmoRadius);

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(_gzForwardGroundTouch, _gizmoRadius);

            if (State.isForwardBlocked)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(_gzForwardTouch, _gizmoRadius);
            }

            Gizmos.color = Color.green;
            Gizmos.DrawLine(_gzGroundTouch, _gzGroundTouch + Current.groundCross * 2f);

            Gizmos.color = Color.black;
            Gizmos.DrawLine(transform.position, transform.position + _gzRotatedWorldMoveDir);

            if (State.isOnSteepSlope)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, transform.position + _gzSlideDir);
            }
        }

        #endregion
    }
}