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

            [HideInInspector] public Transform walker;
        }
        [Serializable]
        public class CheckOption
        {
            [Tooltip("지면으로 체크할 레이어 설정")]
            public LayerMask groundLayerMask = -1;

            [Range(0.1f, 3.0f), Tooltip("지면 감지 거리")]
            public float groundCheckDistance = 2.0f;

            [Range(0.01f, 0.3f), Tooltip("전방 감지 거리")]
            public float forwardCheckDistance = 0.1f;

            [Range(15f, 60f), Tooltip("등반 가능한 경사각")]
            public float maxSlopeAngle = 50f;

            [Range(1f, 3f), Tooltip("경사로 이동속도 변화율")]
            public float slopeSpeedChangeRate = 2f;
        }
        [Serializable]
        public class MovementOption
        {
            [Range(1f, 10f), Tooltip("이동속도")]
            public float speed = 3f;

            [Range(1f, 3f), Tooltip("달리기 이동속도 증가 계수")]
            public float runningCoef = 1.5f;

            [Range(1f, 50f), Tooltip("점프 강도")]
            public float jumpForce = 5.5f;

            [Range(0.0f, 2.0f), Tooltip("점프 쿨타임")]
            public float jumpCooldown = 1.0f;
        }
        [Serializable]
        public class CurrentState
        {
            public bool isMoving;
            public bool isRunning;
            public bool isGrounded;
            public bool isJumpTriggered;
            public bool isForwardBlocked;
        }
        [Serializable]
        public class CurrentValue
        {
            public Vector3 worldMoveDir;
            public Vector3 groundNormal;
            public Vector3 groundCross;
            public Vector3 finalVelocity;

            public float jumpCooldown;
            public float groundSlopeAngle;  // 현재 바닥의 경사각
            public float forwardSlopeAngle; // 캐릭터가 바라보는 방향의 경사각
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
            CheckGround();
            CheckFront();
            Move();

            if(Current.jumpCooldown > 0f)
                Current.jumpCooldown -= Time.fixedDeltaTime;
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
        public void SetWalker(Transform walkerTransform)
        {
            Com.walker = walkerTransform;
        }
        public void SetMovement(in Vector3 worldMoveDir, bool isRunning)
        {
            Current.worldMoveDir = worldMoveDir;
            State.isMoving = worldMoveDir.sqrMagnitude > 0.1f;
            State.isRunning = isRunning;
        }
        public bool SetJump()
        {
            if(State.isGrounded == false)
                return false;

            State.isJumpTriggered = true;
            return Current.jumpCooldown <= 0f;
        }

        public bool IsGrounded() => State.isGrounded;

        #endregion
        /***********************************************************************
        *                               Private Methods
        ***********************************************************************/
        #region .

        private void CheckGround()
        {
            Current.groundNormal = Vector3.up;
            Current.groundSlopeAngle = 0f;
            Current.forwardSlopeAngle = 0f;

            bool sweep =
                Com.subRBody.SweepTest(Vector3.down, out var hit, COption.groundCheckDistance, QueryTriggerInteraction.Ignore);

            State.isGrounded = false;

            if (sweep)
            {
                Current.groundNormal = hit.normal;

                // 현재 위치한 지면의 경사각 구하기(캐릭터 회전 고려)
                Current.groundSlopeAngle = Vector3.Angle(Current.groundNormal, Vector3.up);
                Current.forwardSlopeAngle = Vector3.Angle(Current.groundNormal, Com.walker.forward) - 90f;

                State.isGrounded = 
                    (hit.distance < _capsuleHeightDiff) &&
                    Current.groundSlopeAngle < COption.maxSlopeAngle;

                _groundTouch = hit.point;
            }

            // 월드 이동벡터 회전축
            Current.groundCross = Vector3.Cross(Current.groundNormal, Vector3.up);
        }

        private void CheckFront()
        {
            bool sweep =
                Com.subRBody.SweepTest(Com.walker.forward, out var hit, COption.forwardCheckDistance, QueryTriggerInteraction.Ignore);

            State.isForwardBlocked = false;
            if (sweep)
            {
                _forwardTouch = hit.point;

                float frontObstacleAngle = Vector3.Angle(hit.normal, Vector3.up);

                State.isForwardBlocked = frontObstacleAngle > COption.maxSlopeAngle;
            }
        }

        private void Move()
        {
            // 1. 속력 계산
            float speed = 0f;
            if (State.isMoving)
            {
                speed = MOption.speed;
                if(State.isRunning)
                    speed *= MOption.runningCoef;
            }

            // 2. XZ 이동속도 계산
            if (State.isForwardBlocked) // 전방 이동 불가능한 경우
            {
                Current.finalVelocity = Vector3.zero;
            }
            else
            {
                Current.finalVelocity = Current.worldMoveDir * speed;
            }

            // 3. 벡터 회전
            if (State.isMoving && State.isGrounded)
            {
                // 중력 계산
                Current.finalVelocity += Physics.gravity * 0.2f;

                // 벡터 회전
                Current.finalVelocity =
                    Quaternion.AngleAxis(-Current.groundSlopeAngle, Current.groundCross) * Current.finalVelocity;

                _rotatedWorldMoveDir =
                    Quaternion.AngleAxis(-Current.groundSlopeAngle, Current.groundCross) * Current.worldMoveDir;
            }

            // 4. 리지드바디 중력 적용 여부 설정
            Com.rBody.useGravity = !State.isGrounded;

            // 5. 최종 속도 계산
            if (State.isGrounded)
            {
                Com.rBody.velocity = Current.finalVelocity;
            }
            else
            {
                Com.rBody.velocity =
                    new Vector3(Current.finalVelocity.x, Com.rBody.velocity.y, Current.finalVelocity.z);
            }

            // 6. 점프
            if (State.isJumpTriggered && Current.jumpCooldown <= 0f)
            {
                Com.rBody.useGravity = false;

                // 하강 중 점프 시 속도가 합산되지 않도록 속도 초기화
                Com.rBody.velocity = Vector3.zero;
                Com.rBody.AddForce(Vector3.up * MOption.jumpForce, ForceMode.VelocityChange);

                // 점프 쿨타임, 트리거 초기화
                Current.jumpCooldown = MOption.jumpCooldown;
                State.isJumpTriggered = false;
            }
        }

        #endregion
        /***********************************************************************
        *                               Gizmos
        ***********************************************************************/
        #region .
        private Vector3 _groundTouch;
        private Vector3 _forwardTouch;
        private Vector3 _rotatedWorldMoveDir;

        [Header("Gizmos Option"), SerializeField, Range(0.1f, 2f)]
        private float _gizmoRadius = 0.1f;

        private void OnDrawGizmos()
        {
            if (Application.isPlaying == false) return;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_groundTouch, _gizmoRadius);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_forwardTouch, _gizmoRadius);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(_groundTouch, _groundTouch + Current.groundCross * 2f);

            Gizmos.color = Color.black;
            Gizmos.DrawLine(transform.position, transform.position + _rotatedWorldMoveDir);
        }

        #endregion
    }
}