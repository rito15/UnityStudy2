using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-02-25 PM 8:32:15
// 작성자 : Rito

// NOTE : Rigidbody.Move 테스트

namespace Rito.CharacterControl.Test
{
    public class CcMovement : MonoBehaviour, IMovement3D
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

            [Range(0.01f, 0.5f), Tooltip("전방 감지 거리")]
            public float forwardCheckDistance = 0.1f;
        }
        [Serializable]
        public class MovementOption
        {
            [Range(1f, 10f), Tooltip("이동속도")]
            public float speed = 5f;

            [Range(1f, 3f), Tooltip("달리기 이동속도 증가 계수")]
            public float runningCoef = 1.5f;

            [Range(1f, 10f), Tooltip("점프 강도")]
            public float jumpForce = 4.2f;

            [Range(0.0f, 2.0f), Tooltip("점프 쿨타임")]
            public float jumpCooldown = 0.6f;

            [Range(0, 3), Tooltip("점프 허용 횟수")]
            public int maxJumpCount = 1;

            [Range(15f, 70f), Tooltip("등반 가능한 경사각")]
            public float maxSlopeAngle = 50f;

            [Range(0f, 4f), Tooltip("경사로 이동속도 변화율(가속/감속)")]
            public float slopeAccel = 1f;
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
            public bool isOutOfControl; // 제어 불가 상태
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
            public float outOfControllDuration;

            [Space]
            public float groundDistance;
            public float groundSlopeAngle;         // 현재 바닥의 경사각
            public float groundVerticalSlopeAngle; // 수직으로 재측정한 경사각
            public float forwardSlopeAngle; // 캐릭터가 바라보는 방향의 경사각
            public float slopeAccel;        // 경사로 인한 가속/감속 비율

            [Space]
            public float gravity; // 직접 제어하는 중력값
        }

        #endregion
        /***********************************************************************
        *                               Public Properties
        ***********************************************************************/
        #region .
        public bool IsMoving => State.isMoving;
        public bool IsGrounded => State.isGrounded;
        public float DistanceFromGround => Current.groundDistance;

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

        private Components Com => _components;
        private CheckOption COption => _checkOptions;
        private MovementOption MOption => _moveOptions;
        private CurrentState State => _currentStates;
        private CurrentValue Current => _currentValues;


        private float _capsuleHeightDiff;
        private float _fixedDeltaTime;
        private Vector3 _nextMovePos;

        #endregion
        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .
        private void Start()
        {
            //InitComponents();
            //InitSubComponents();
            Destroy(gameObject.GetComponent<Rigidbody>());
            Destroy(gameObject.GetComponent<CapsuleCollider>());

            cc = gameObject.GetComponent<CharacterController>();

            Debug.Log(cc.attachedRigidbody);
        }

        CharacterController cc;

        Vector3 moveDir;


        [Space, SerializeField, Range(-9.81f, -1f)]
        private float _ccGravity = -9.81f;

        private void Update()
        {
            moveDir.x = Current.worldMoveDir.x;
            moveDir.z = Current.worldMoveDir.z;
            if (State.isRunning) moveDir *= 2f;

            //if (Input.GetKeyDown(KeyCode.V) && cc.isGrounded)
            //{
            //    moveDir.y = MOption.jumpForce;
            //}

            // 플레이어가 땅을 밟고 있지 않다면
            // y축 이동방향에 gravity * Time.deltaTime을 더해준다
            if (cc.isGrounded == false)
            {
                moveDir.y += _ccGravity * Time.deltaTime;
            }

            cc.Move(moveDir * MOption.speed * Time.deltaTime);

        }

        private void FixedUpdate()
        {
            _fixedDeltaTime = Time.fixedDeltaTime;

            //UpdateValues();
            //CheckGroundSweepTest();
            //CheckForwardSweepTest();

            //Move();
            //CalculateGravity();
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
            Com.rBody.interpolation = RigidbodyInterpolation.Interpolate;
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
        *                               Private Check Methods
        ***********************************************************************/
        #region .

        /// <summary> 하단 지면 검사 </summary>
        private void CheckGroundSweepTest()
        {
            Current.groundDistance = float.MaxValue;
            Current.groundNormal = Vector3.up;
            Current.groundSlopeAngle = 0f;
            Current.forwardSlopeAngle = 0f;

            // D : down / FD : Foward Down

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

                // 현재 위치한 지면의 경사각 구하기(캐릭터 이동방향 고려)
                Current.groundSlopeAngle = Vector3.Angle(Current.groundNormal, Vector3.up);
                Current.forwardSlopeAngle = Vector3.Angle(Current.groundNormal, Current.worldMoveDir) - 90f;

                State.isOnSteepSlope = Current.groundSlopeAngle >= MOption.maxSlopeAngle;

                // 경사각 이중검증 (수직 레이캐스트) : 뾰족하거나 각진 부분 체크
                if (State.isOnSteepSlope)
                {
                    Vector3 ro = hitD.point + Vector3.up * 0.1f;
                    Vector3 rd = Vector3.down;
                    bool rayD =
                        //Physics.Raycast(ro, rd, out var hitRayD, 0.2f, COption.groundLayerMask, QueryTriggerInteraction.Ignore);
                        Physics.SphereCast(ro, 0.09f, rd, out var hitRayD, 0.2f, COption.groundLayerMask, QueryTriggerInteraction.Ignore);

                    Current.groundVerticalSlopeAngle = rayD ? Vector3.Angle(hitRayD.normal, Vector3.up) : Current.groundSlopeAngle;

                    State.isOnSteepSlope = Current.groundVerticalSlopeAngle >= MOption.maxSlopeAngle;
                }

                State.isGrounded =
                    (hitD.distance < _capsuleHeightDiff) && !State.isOnSteepSlope;

                Current.groundDistance = Mathf.Max(transform.position.y - hitD.point.y, 0f);

            }

            // 월드 이동벡터 회전축
            Current.groundCross = Vector3.Cross(Current.groundNormal, Vector3.up);
        }

        /// <summary> 전방 장애물 검사 </summary>
        private void CheckForwardSweepTest()
        {
            bool sweep =
                Com.subRBody.SweepTest(Current.worldMoveDir + Vector3.down * 0.2f,
                out var hit, COption.forwardCheckDistance, QueryTriggerInteraction.Ignore);

            State.isForwardBlocked = false;
            if (sweep)
            {
                Current.forwardSlopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                State.isForwardBlocked = Current.forwardSlopeAngle >= MOption.maxSlopeAngle;
            }
        }

        #endregion

        /***********************************************************************
        *                               Private Physics Methods
        ***********************************************************************/
        #region .
        private void Move()
        {
            if(State.isForwardBlocked) return;
            //if(State.isOnSteepSlope) return;

            Vector3 nextMoveDir = Current.worldMoveDir;
                //State.isOnSteepSlope ? Current.worldMoveDir :
                //Quaternion.AngleAxis(-Current.groundSlopeAngle, Current.groundCross)
                //    * Current.worldMoveDir;

            _nextMovePos = Com.rBody.position +
                nextMoveDir * MOption.speed * _fixedDeltaTime;

            Com.rBody.MovePosition(_nextMovePos);
        }

        private void CalculateGravity()
        {
            Com.rBody.useGravity = true; return;

            if (State.isOnSteepSlope || State.isForwardBlocked)
            {
                Com.rBody.velocity -= Vector3.up * 9.81f * _fixedDeltaTime;
            }
            else
            {
                Com.rBody.velocity -= Current.groundNormal * 9.81f * _fixedDeltaTime;
            }
        }

        #endregion

        /***********************************************************************
        *                               Public
        ***********************************************************************/
        #region .

        float IMovement3D.GetDistanceFromGround() => 0f;
        bool IMovement3D.IsMoving() => State.isMoving;
        bool IMovement3D.IsGrounded() => State.isGrounded;
        void IMovement3D.StopMoving() { }
        void IMovement3D.KnockBack(in Vector3 force, float time) { }

        void IMovement3D.SetMovement(in Vector3 worldMove, bool runningState)
        {
            Current.worldMoveDir = !runningState ? worldMove : worldMove * MOption.runningCoef;
        }

        bool IMovement3D.SetJump()
        {
            //Com.rBody.AddForce(MOption.jumpForce * Current.groundNormal, ForceMode.VelocityChange);
            moveDir.y = MOption.jumpForce;

            return true;
        }

        #endregion
    }
}