using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-02-21 PM 8:23:22
// 작성자 : Rito

// 단점 : 오를 수 없는 경사면으로 이동하려고 시도할 경우 지터링 발생

namespace Rito.CharacterControl
{
    public class PhysicsBasedMovement : MonoBehaviour, IMovement3D
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
        }
        [Serializable]
        public class CheckOption
        {
            [Tooltip("지면으로 체크할 레이어 설정")]
            public LayerMask groundLayerMask = -1;

            [Range(0.01f, 0.5f), Tooltip("전방 감지 거리")]
            public float forwardCheckDistance = 0.1f;

            [Range(0.1f, 10.0f), Tooltip("지면 감지 거리")]
            public float groundCheckDistance = 2.0f;

            [Range(0.0f, 0.1f), Tooltip("지면 인식 허용 거리")]
            public float groundCheckThreshold = 0.01f;
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

            [Range(1f, 75f), Tooltip("등반 가능한 경사각")]
            public float maxSlopeAngle = 50f;

            [Range(0f, 4f), Tooltip("경사로 이동속도 변화율(가속/감속)")]
            public float slopeAccel = 1f;

            [Range(-9.81f, 0f), Tooltip("중력")]
            public float gravity = -9.81f;
        }
        [Serializable]
        public class CurrentState
        {
            public bool isMoving;
            public bool isRunning;
            public bool isGrounded;
            public bool isOnSteepSlope;   // 등반 불가능한 경사로에 올라와 있음
            public bool isJumpTriggered;
            public bool isJumping;
            public bool isForwardBlocked; // 전방에 장애물 존재
            public bool isOutOfControl;   // 제어 불가 상태
        }
        [Serializable]
        public class CurrentValue
        {
            public Vector3 worldMoveDir;
            public Vector3 groundNormal;
            public Vector3 groundCross;
            public Vector3 horizontalVelocity;

            [Space]
            public float jumpCooldown;
            public int   jumpCount;
            public float outOfControllDuration;

            [Space]
            public float groundDistance;
            public float groundSlopeAngle;         // 현재 바닥의 경사각
            public float groundVerticalSlopeAngle; // 수직으로 재측정한 경사각
            public float forwardSlopeAngle; // 캐릭터가 바라보는 방향의 경사각
            public float slopeAccel;        // 경사로 인한 가속/감속 비율

            [Space]
            public float gravity;
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

        private Components Com => _components;
        private CheckOption COption => _checkOptions;
        private MovementOption MOption => _moveOptions;
        private CurrentState State => _currentStates;
        private CurrentValue Current => _currentValues;


        private float _capsuleRadiusDiff;
        private float _fixedDeltaTime;

        private float _castRadius; // Sphere, Capsule 레이캐스트 반지름
        private Vector3 CapsuleTopCenterPoint 
            => new Vector3(transform.position.x, transform.position.y + Com.capsule.height - Com.capsule.radius, transform.position.z);
        private Vector3 CapsuleBottomCenterPoint 
            => new Vector3(transform.position.x, transform.position.y + Com.capsule.radius, transform.position.z);

        #endregion
        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .
        private void Start()
        {
            InitComponents();
        }

        private void FixedUpdate()
        {
            _fixedDeltaTime = Time.fixedDeltaTime;

            CheckGround();
            CheckForward();

            UpdatePhysics();
            UpdateValues();

            CalculateMovements();
            ApplyMovementsToRigidbody();
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
            Com.rBody.useGravity = false; // 중력 직접 제어

            _castRadius = Com.capsule.radius * 0.9f;
            _capsuleRadiusDiff = Com.capsule.radius - _castRadius + 0.05f;
        }

        #endregion
        /***********************************************************************
        *                               Public Methods
        ***********************************************************************/
        #region .

        bool IMovement3D.IsMoving() => State.isMoving;
        bool IMovement3D.IsGrounded() => State.isGrounded;
        float IMovement3D.GetDistanceFromGround() => Current.groundDistance;

        void IMovement3D.SetMovement(in Vector3 worldMoveDir, bool isRunning)
        {
            Current.worldMoveDir = worldMoveDir;
            State.isMoving = worldMoveDir.sqrMagnitude > 0.01f;
            State.isRunning = isRunning;
        }
        bool IMovement3D.SetJump()
        {
            // 첫 점프는 지면 위에서만 가능
            if (!State.isGrounded && Current.jumpCount == 0) return false;

            // 점프 쿨타임, 횟수 확인
            if (Current.jumpCooldown > 0f) return false;
            if (Current.jumpCount >= MOption.maxJumpCount) return false;

            // 접근 불가능 경사로에서 점프 불가능
            if (State.isOnSteepSlope) return false;

            State.isJumpTriggered = true;
            return true;
        }

        void IMovement3D.StopMoving()
        {
            Current.worldMoveDir = Vector3.zero;
            State.isMoving = false;
            State.isRunning = false;
        }

        void IMovement3D.KnockBack(in Vector3 force, float time)
        {
            SetOutOfControl(time);
            Com.rBody.AddForce(force, ForceMode.Impulse);
        }

        public void SetOutOfControl(float time)
        {
            Current.outOfControllDuration = time;
            ResetJump();
        }

        #endregion
        /***********************************************************************
        *                               Private Methods
        ***********************************************************************/
        #region .

        private void ResetJump()
        {
            Current.jumpCooldown = 0f;
            Current.jumpCount = 0;
            State.isJumping = false;
            State.isJumpTriggered = false;
        }

        /// <summary> 하단 지면 검사 </summary>
        private void CheckGround()
        {
            Current.groundDistance = float.MaxValue;
            Current.groundNormal = Vector3.up;
            Current.groundSlopeAngle = 0f;
            Current.forwardSlopeAngle = 0f;

            bool cast =
                Physics.SphereCast(CapsuleBottomCenterPoint, _castRadius, Vector3.down, out var hit, COption.groundCheckDistance, COption.groundLayerMask, QueryTriggerInteraction.Ignore);

            State.isGrounded = false;

            if (cast)
            {
                // 지면 노멀벡터 초기화
                Current.groundNormal = hit.normal;

                // 현재 위치한 지면의 경사각 구하기(캐릭터 이동방향 고려)
                Current.groundSlopeAngle = Vector3.Angle(Current.groundNormal, Vector3.up);
                Current.forwardSlopeAngle = Vector3.Angle(Current.groundNormal, Current.worldMoveDir) - 90f;

                State.isOnSteepSlope = Current.groundSlopeAngle >= MOption.maxSlopeAngle;

                // 경사각 이중검증 (수직 레이캐스트) : 뾰족하거나 각진 부분 체크
                //if (State.isOnSteepSlope)
                //{
                //    Vector3 ro = hit.point + Vector3.up * 0.1f;
                //    Vector3 rd = Vector3.down;
                //    bool rayD = 
                //        Physics.SphereCast(ro, 0.09f, rd, out var hitRayD, 0.2f, COption.groundLayerMask, QueryTriggerInteraction.Ignore);

                //    Current.groundVerticalSlopeAngle = rayD ? Vector3.Angle(hitRayD.normal, Vector3.up) : Current.groundSlopeAngle;

                //    State.isOnSteepSlope = Current.groundVerticalSlopeAngle >= MOption.maxSlopeAngle;
                //}

                Current.groundDistance = Mathf.Max(hit.distance - _capsuleRadiusDiff - COption.groundCheckThreshold, 0f);

                State.isGrounded =
                    (Current.groundDistance <= 0.0001f) && !State.isOnSteepSlope;

                GzUpdateValue(ref _gzGroundTouch, hit.point);
            }

            // 월드 이동벡터 회전축
            Current.groundCross = Vector3.Cross(Current.groundNormal, Vector3.up);
        }

        /// <summary> 전방 장애물 검사 : 레이어 관계 없이 trigger가 아닌 모든 장애물 검사 </summary>
        private void CheckForward()
        {
            bool cast =
                Physics.CapsuleCast(CapsuleBottomCenterPoint, CapsuleTopCenterPoint, _castRadius, Current.worldMoveDir + Vector3.down * 0.1f,
                    out var hit, COption.forwardCheckDistance, -1, QueryTriggerInteraction.Ignore);

            State.isForwardBlocked = false;
            if (cast)
            {
                float forwardObstacleAngle = Vector3.Angle(hit.normal, Vector3.up);
                State.isForwardBlocked = forwardObstacleAngle >= MOption.maxSlopeAngle;

                GzUpdateValue(ref _gzForwardTouch, hit.point);
            }
        }

        private void UpdatePhysics()
        {
            // Custom Gravity, Jumping State
            if (State.isGrounded)
            {
                Current.gravity = 0f;

                Current.jumpCount = 0;
                State.isJumping = false;
            }
            else
            {
                Current.gravity += _fixedDeltaTime * MOption.gravity;
            }
        }

        private void UpdateValues()
        {
            // Calculate Jump Cooldown
            if (Current.jumpCooldown > 0f)
                Current.jumpCooldown -= _fixedDeltaTime;

            // Out Of Control
            State.isOutOfControl = Current.outOfControllDuration > 0f;

            if (State.isOutOfControl)
            {
                Current.outOfControllDuration -= _fixedDeltaTime;
                Current.worldMoveDir = Vector3.zero;
            }
        }

        private void CalculateMovements()
        {
            if (State.isOutOfControl)
            {
                Current.horizontalVelocity = Vector3.zero;
                return;
            }

            // 0. 가파른 경사면에 있는 경우 : 꼼짝말고 미끄럼틀 타기
            //if (State.isOnSteepSlope && Current.groundDistance < 0.1f)
            //{
            //    DebugMark(0);

            //    Current.horizontalVelocity =
            //        Quaternion.AngleAxis(90f - Current.groundSlopeAngle, Current.groundCross) * (Vector3.up * Current.gravity);

            //    Com.rBody.velocity = Current.horizontalVelocity;

            //    return;
            //}

            // 1. 점프
            if (State.isJumpTriggered && Current.jumpCooldown <= 0f)
            {
                DebugMark(1);

                Current.gravity = MOption.jumpForce;

                // 점프 쿨타임, 트리거 초기화
                Current.jumpCooldown = MOption.jumpCooldown;
                State.isJumpTriggered = false;
                State.isJumping = true;

                Current.jumpCount++;
            }

            // 2. XZ 이동속도 계산
            // 공중에서 전방이 막힌 경우 제한 (지상에서는 벽에 붙어서 이동할 수 있도록 허용)
            if (State.isForwardBlocked && !State.isGrounded || State.isJumping && State.isGrounded) 
            {
                DebugMark(2);

                Current.horizontalVelocity = Vector3.zero;
            }
            else // 이동 가능한 경우 : 지상 or 전방이 막히지 않음
            {
                DebugMark(3);

                float speed = !State.isMoving  ? 0f :
                              !State.isRunning ? MOption.speed :
                                                 MOption.speed * MOption.runningCoef;

                Current.horizontalVelocity = Current.worldMoveDir * speed;
            }

            // 3. XZ 벡터 회전
            // 지상이거나 지면에 가까운 높이
            if (State.isGrounded || Current.groundDistance < COption.groundCheckDistance && !State.isJumping)
            {
                if (State.isMoving && !State.isForwardBlocked)
                {
                    DebugMark(4);

                    // 경사로 인한 가속/감속
                    if (MOption.slopeAccel > 0f)
                    {
                        bool isPlus = Current.forwardSlopeAngle >= 0f;
                        float absFsAngle = isPlus ? Current.forwardSlopeAngle : -Current.forwardSlopeAngle;
                        float accel = MOption.slopeAccel * absFsAngle * 0.01111f + 1f;
                        Current.slopeAccel = !isPlus ? accel : 1.0f / accel;

                        Current.horizontalVelocity *= Current.slopeAccel;
                    }

                    // 벡터 회전 (경사로)
                    Current.horizontalVelocity =
                        Quaternion.AngleAxis(-Current.groundSlopeAngle, Current.groundCross) * Current.horizontalVelocity;
                }
            }

            GzUpdateValue(ref _gzRotatedWorldMoveDir, Current.horizontalVelocity * 0.2f);
        }

        /// <summary> 리지드바디 최종 속도 적용 </summary>
        private void ApplyMovementsToRigidbody()
        {
            if (State.isOutOfControl)
            {
                Com.rBody.velocity = new Vector3(Com.rBody.velocity.x, Current.gravity, Com.rBody.velocity.z);
                return;
            }

            Com.rBody.velocity = Current.horizontalVelocity + Vector3.up * (Current.gravity);
        }

        #endregion
        /***********************************************************************
        *                               Debugs
        ***********************************************************************/
        #region .

        public bool _debugOn;
        public int _debugIndex;

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void DebugMark(int index)
        {
            if(!_debugOn) return;
            Debug.Log("MARK - " + index);
            _debugIndex = index;
        }

        #endregion
        /***********************************************************************
        *                               Gizmos, GUI
        ***********************************************************************/
        #region .

        private Vector3 _gzGroundTouch;
        private Vector3 _gzForwardTouch;
        private Vector3 _gzRotatedWorldMoveDir;

        [Header("Gizmos Option")]
        public bool _showGizmos = true;

        [SerializeField, Range(0.01f, 2f)]
        private float _gizmoRadius = 0.05f;

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void OnDrawGizmos()
        {
            if (Application.isPlaying == false) return;
            if (!_showGizmos) return;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_gzGroundTouch, _gizmoRadius);

            if (State.isForwardBlocked)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(_gzForwardTouch, _gizmoRadius);
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(_gzGroundTouch - Current.groundCross, _gzGroundTouch + Current.groundCross);

            Gizmos.color = Color.black;
            Gizmos.DrawLine(transform.position, transform.position + _gzRotatedWorldMoveDir);

            Gizmos.color = new Color(0.5f, 1.0f, 0.8f, 0.8f);
            Gizmos.DrawWireSphere(CapsuleTopCenterPoint, _castRadius);
            Gizmos.DrawWireSphere(CapsuleBottomCenterPoint, _castRadius);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void GzUpdateValue<T>(ref T variable, in T value)
        {
            variable = value;
        }



        [SerializeField, Space]
        private bool _showGUI = true;
        [SerializeField]
        private int _guiTextSize = 28;

        private float _prevForwardSlopeAngle;

        private void OnGUI()
        {
            if(!_showGUI) return;

            GUIStyle labelStyle = GUI.skin.label;
            labelStyle.normal.textColor = Color.yellow;
            labelStyle.fontSize = Math.Max(_guiTextSize, 20);

            _prevForwardSlopeAngle = Current.forwardSlopeAngle == -90f ? _prevForwardSlopeAngle : Current.forwardSlopeAngle;

            var oldColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.5f);
            GUI.Box(new Rect(40, 40, 420, 260), "");
            GUI.color = oldColor;

            GUILayout.BeginArea(new Rect(50, 50, 1000, 500));
            GUILayout.Label($"Ground Height : {Mathf.Min(Current.groundDistance, 99.99f): 00.00}", labelStyle);
            GUILayout.Label($"Slope Angle(Ground)  : {Current.groundSlopeAngle: 00.00}", labelStyle);
            GUILayout.Label($"Slope Angle(Forward) : {_prevForwardSlopeAngle: 00.00}", labelStyle);
            GUILayout.Label($"Allowed Slope Angle : {MOption.maxSlopeAngle: 00.00}", labelStyle);
            GUILayout.Label($"Current Slope Accel : {Current.slopeAccel: 00.00}", labelStyle);
            GUILayout.Label($"Current Speed Mag  : {Current.horizontalVelocity.magnitude: 00.00}", labelStyle);
            GUILayout.EndArea();

            float sWidth = Screen.width;
            float sHeight = Screen.height;

            GUIStyle RTLabelStyle = GUI.skin.label;
            RTLabelStyle.fontSize = 20;
            RTLabelStyle.normal.textColor = Color.green;

            oldColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            GUI.Box(new Rect(sWidth - 355f, 5f, 340f, 100f), "");
            GUI.color = oldColor;

            float yPos = 10f;
            GUI.Label(new Rect(sWidth - 350f, yPos, 150f, 30f), $"Speed : {MOption.speed: 00.00}", RTLabelStyle);
            MOption.speed = GUI.HorizontalSlider(new Rect(sWidth - 200f, yPos+10f, 180f, 20f), MOption.speed, 1f, 10f);

            yPos += 20f;
            GUI.Label(new Rect(sWidth - 350f, yPos, 150f, 30f), $"Jump : {MOption.jumpForce: 00.00}", RTLabelStyle);
            MOption.jumpForce = GUI.HorizontalSlider(new Rect(sWidth - 200f, yPos+ 10f, 180f, 20f), MOption.jumpForce, 1f, 10f);

            yPos += 20f;
            GUI.Label(new Rect(sWidth - 350f, yPos, 150f, 30f), $"Jump Count : {MOption.maxJumpCount: 0}", RTLabelStyle);
            MOption.maxJumpCount = (int)GUI.HorizontalSlider(new Rect(sWidth - 200f, yPos+ 10f, 180f, 20f), MOption.maxJumpCount, 1f, 3f);

            yPos += 20f;
            GUI.Label(new Rect(sWidth - 350f, yPos, 150f, 30f), $"Max Slope : {MOption.maxSlopeAngle: 00}", RTLabelStyle);
            MOption.maxSlopeAngle = (int)GUI.HorizontalSlider(new Rect(sWidth - 200f, yPos+ 10f, 180f, 20f), MOption.maxSlopeAngle, 1f, 75f);

            labelStyle.fontSize = Math.Max(_guiTextSize, 20);
        }

        #endregion
    }
}