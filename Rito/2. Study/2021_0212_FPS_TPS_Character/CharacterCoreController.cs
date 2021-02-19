using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-02-12 PM 7:45:19
// 작성자 : Rito

namespace Rito.FpsTpsCharacter
{
    public class CharacterCoreController : MonoBehaviour
    {
        /***********************************************************************
        *                               Definitions
        ***********************************************************************/
        #region .
        public enum CameraType { FpCamera, TpCamera };

        [Serializable]
        public class Components
        {
            public Camera tpCamera;
            public Camera fpCamera;

            [HideInInspector] public Transform tpRig;
            [HideInInspector] public Transform fpRoot;
            [HideInInspector] public Transform fpRig;

            [HideInInspector] public GameObject tpCamObject;
            [HideInInspector] public GameObject fpCamObject;

            [HideInInspector] public Rigidbody rBody;
            [HideInInspector] public Animator anim;
        }
        [Serializable]
        public class KeyOption
        {
            public KeyCode moveForward  = KeyCode.W;
            public KeyCode moveBackward = KeyCode.S;
            public KeyCode moveLeft     = KeyCode.A;
            public KeyCode moveRight    = KeyCode.D;
            public KeyCode run  = KeyCode.LeftShift;
            public KeyCode jump = KeyCode.Space;
            public KeyCode switchCamera = KeyCode.Tab;
            public KeyCode showCursor = KeyCode.LeftAlt;
        }
        [Serializable]
        public class MovementOption
        {
            [Range(1f, 10f), Tooltip("이동속도")]
            public float speed = 3f;

            //[Range(0.01f, 0.3f), Tooltip("이동 가속 계수")]
            //public float acceleration = 0.1f;

            [Range(1f, 3f), Tooltip("달리기 이동속도 증가 계수")]
            public float runningCoef = 1.5f;

            [Range(1f, 10f), Tooltip("점프 강도")]
            public float jumpForce = 5.5f;

            [Tooltip("지면으로 체크할 레이어 설정")]
            public LayerMask groundLayerMask = -1;

            [Range(0.0f, 2.0f), Tooltip("점프 쿨타임")]
            public float jumpCooldown = 1.0f;
        }
        [Serializable]
        public class CheckOption
        {
            [Range(0.1f, 4.0f), Tooltip("전방 감지 거리")]
            public float forwardCheckDist = 1.0f;

            [Range(0.1f, 4.0f), Tooltip("하단 감지 거리 - 이동벡터 회전용")]
            public float downCheckDist = 2.0f;
        }
        [Serializable]
        public class CameraOption
        {
            [Tooltip("게임 시작 시 카메라")]
            public CameraType initialCamera;

            [Range(1f, 10f), Tooltip("카메라 상하좌우 회전 속도")]
            public float rotationSpeed = 2f;

            [Range(-90f, 0f), Tooltip("올려다보기 제한 각도")]
            public float lookUpDegree = -60f;

            [Range(0f, 75f), Tooltip("내려다보기 제한 각도")]
            public float lookDownDegree = 75f;

            [Range(0f, 3.5f), Space, Tooltip("줌 확대 최대 거리")]
            public float zoomInDistance = 3f;

            [Range(0f, 5f), Tooltip("줌 축소 최대 거리")]
            public float zoomOutDistance = 3f;

            [Range(1f, 30f), Tooltip("줌 속도")]
            public float zoomSpeed = 20f;

            [Range(0.01f, 0.5f), Tooltip("줌 가속")]
            public float zoomAccel = 0.1f;
        }
        [Serializable]
        public class AnimatorOption
        {
            public string paramMoveX = "Move X";
            public string paramMoveZ = "Move Z";
            public string paramDistY = "Dist Y";
            public string paramGrounded = "Grounded";
            public string paramJump = "Jump";
        }
        [Serializable]
        public class CharacterState
        {
            public bool isCurrentFp;
            public bool isMoving;
            public bool isRunning;
            public bool isGrounded;
            public bool isCursorActive;
        }

        #endregion
        /***********************************************************************
        *                               Fields, Properties
        ***********************************************************************/
        #region .
        public Components Com => _components;
        public KeyOption Key => _keyOption;
        public CheckOption Check => _checkOption;
        public MovementOption MoveOption => _movementOption;
        public CameraOption   CamOption  => _cameraOption;
        public AnimatorOption AnimOption => _animatorOption;
        public CharacterState State => _state;

        [SerializeField] private Components _components = new Components();
        [Space, SerializeField] private KeyOption _keyOption = new KeyOption();
        [Space, SerializeField] private CheckOption _checkOption = new CheckOption();
        [Space, SerializeField] private MovementOption _movementOption = new MovementOption();
        [Space, SerializeField] private CameraOption   _cameraOption   = new CameraOption();
        [Space, SerializeField] private AnimatorOption _animatorOption = new AnimatorOption();
        [Space, SerializeField] private CharacterState _state = new CharacterState();

        /// <summary> Time.deltaTime 항상 저장 </summary>
        private float _deltaTime;

        /// <summary> 키보드 WASD 입력으로 얻는 로컬 이동 벡터 </summary>
        [SerializeField]
        private Vector3 _moveDir;

        /// <summary> 월드 이동 벡터 </summary>
        private Vector3 _worldMove;

        /// <summary> 마우스 움직임을 통해 얻는 회전 값 </summary>
        private Vector2 _rotation;

        /// <summary> 지면을 향하는 SphereCast 반지름 </summary>
        [SerializeField] private float _groundCheckRadius;

        [SerializeField]
        private float _distFromGround;

        // Animation Params
        private float _moveX;
        private float _moveZ;


        /// <summary> TP 카메라 ~ Rig 초기 거리 </summary>
        private float _tpCamZoomInitialDistance;

        /// <summary> TP 카메라 휠 입력 값 </summary>
        private float _tpCameraWheelInput = 0;

        /// <summary> 선형보간된 현재 휠 입력 값 </summary>
        private float _currentWheel;

        private float _currentJumpCooldown;


        #endregion

        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .
        private void Awake()
        {
            InitComponents();
            InitSettings();
        }

        private void Update()
        {
            _deltaTime = Time.deltaTime;

            // 1. Check, Key Input
            ShowCursorToggle();
            CameraViewToggle();
            SetValuesByKeyInput();
            CheckDistanceFromGround();

            // 2. Behaviors, Camera Actions
            Rotate();
            Move();
            Jump();
            TpCameraZoom();

            // 3. Updates
            UpdateAnimationParams();
            UpdateCurrentValues();
        }

        #endregion
        /***********************************************************************
        *                               Init Methods
        ***********************************************************************/
        #region .
        private void InitComponents()
        {
            LogNotInitializedComponentError(Com.tpCamera, "TP Camera");
            LogNotInitializedComponentError(Com.fpCamera, "FP Camera");
            TryGetComponent(out Com.rBody);
            Com.anim = GetComponentInChildren<Animator>();

            Com.tpCamObject = Com.tpCamera.gameObject;
            Com.tpRig = Com.tpCamera.transform.parent;
            Com.fpCamObject = Com.fpCamera.gameObject;
            Com.fpRig = Com.fpCamera.transform.parent;
            Com.fpRoot = Com.fpRig.parent;
        }

        private void InitSettings()
        {
            // Rigidbody
            if (Com.rBody)
            {
                // 회전은 트랜스폼을 통해 직접 제어할 것이기 때문에 리지드바디 회전은 제한
                Com.rBody.constraints = RigidbodyConstraints.FreezeRotation;
            }

            // Camera
            var allCams = FindObjectsOfType<Camera>();
            foreach (var cam in allCams)
            {
                cam.gameObject.SetActive(false);
            }
            // 설정한 카메라 하나만 활성화
            State.isCurrentFp = (CamOption.initialCamera == CameraType.FpCamera);
            Com.fpCamObject.SetActive(State.isCurrentFp);
            Com.tpCamObject.SetActive(!State.isCurrentFp);

            // 지면 체크 반지름 설정 : 캡슐 콜라이더 기반
            TryGetComponent(out CapsuleCollider cCol);
            _groundCheckRadius = cCol ? cCol.radius * 0.3f : 0.1f;

            // Zoom
            _tpCamZoomInitialDistance = Vector3.Distance(Com.tpRig.position, Com.tpCamera.transform.position);
        }
        
        #endregion
        /***********************************************************************
        *                               Check Methods
        ***********************************************************************/
        #region .
        private void LogNotInitializedComponentError<T>(T component, string componentName) where T : Component
        {
            if(component == null)
                Debug.LogError($"{componentName} 컴포넌트를 인스펙터에 넣어주세요");
        }

        /// <summary> 땅으로부터의 거리 체크 </summary>
        private void CheckDistanceFromGround()
        {
            Vector3 ro = transform.position + Vector3.up;
            Vector3 rd = Vector3.down;
            Ray ray = new Ray(ro, rd);

            const float rayDist = 500f;
            const float threshold = 0.01f;

            bool cast =
                Physics.SphereCast(ray, _groundCheckRadius, out var hit, rayDist, MoveOption.groundLayerMask);

            _distFromGround = cast ? (hit.distance - 1f + _groundCheckRadius) : float.MaxValue;
            State.isGrounded = _distFromGround <= _groundCheckRadius + threshold;
        }

        private void CheckForwardAndAdjustMoveVector()
        {
            if(State.isGrounded) return;

            Vector3 ro = transform.position + new Vector3(0, 0.1f, 0);
            Vector3 fwd = Com.fpRoot.forward;

            Vector3[] rds = { 
                fwd,
                Quaternion.Euler(new Vector3(0f,  30f, 0f)) * fwd,
                Quaternion.Euler(new Vector3(0f, -30f, 0f)) * fwd,
                Quaternion.Euler(new Vector3(0f,  60f, 0f)) * fwd,
                Quaternion.Euler(new Vector3(0f, -60f, 0f)) * fwd
            };

            float[] dists = {
                Check.forwardCheckDist,
                Check.forwardCheckDist * 0.9f,
                Check.forwardCheckDist * 0.9f,
                Check.forwardCheckDist * 0.7f,
                Check.forwardCheckDist * 0.7f
            };

            for (int i = 0; i < rds.Length; i++)
            {
                bool rayCast = Physics.Raycast(ro, rds[i], out var hit, dists[i], -1);
                if (rayCast)
                {
                    _moveDir = Vector3.zero;
                    Debug.DrawRay(ro, hit.point - ro, Color.red, 0.1f);

                    break;
                }
            }
        }

        //private void CheckForwardAndAdjustMoveVector()
        //{
        //    if(State.isGrounded) return;

        //    const float threshold = 0.5f;

        //    Vector3 ro = transform.position + new Vector3(0, 0.0f, 0);
        //    Vector3 rd = Com.fpRoot.forward;

        //    bool rayCast = Physics.Raycast(ro, rd, out var hit, Check.forwardCheckDist, -1);
        //    if (rayCast)
        //    {
        //        float yAxisDot = hit.distance < threshold ? 
        //            Vector3.Dot(Vector3.up, hit.normal) : 1f;

        //        float ratio = Mathf.Max(0f, (hit.distance)) / Check.forwardCheckDist;

        //        _moveDir *= ratio * yAxisDot;

        //        Debug.DrawRay(ro, hit.point - ro, Color.red, 0.1f);
        //    }
        //}

        #endregion
        /***********************************************************************
        *                               Methods
        ***********************************************************************/
        #region .
        /// <summary> 키보드 입력을 통해 필드 초기화 </summary>
        private void SetValuesByKeyInput()
        {
            float h = 0f, v = 0f;

            if (Input.GetKey(Key.moveForward)) v += 1.0f;
            if (Input.GetKey(Key.moveBackward)) v -= 1.0f;
            if (Input.GetKey(Key.moveLeft)) h -= 1.0f;
            if (Input.GetKey(Key.moveRight)) h += 1.0f;

            _moveDir = new Vector3(h, 0f, v).normalized;
            _rotation = new Vector2(Input.GetAxisRaw("Mouse X"), -Input.GetAxisRaw("Mouse Y"));

            State.isMoving = _moveDir.sqrMagnitude > 0.1f;//h != 0 || v != 0;
            State.isRunning = Input.GetKey(Key.run);

            // Wheel
            _tpCameraWheelInput = Input.GetAxisRaw("Mouse ScrollWheel");
            _currentWheel = Mathf.Lerp(_currentWheel, _tpCameraWheelInput, CamOption.zoomAccel);
        }

        private void Rotate()
        {
            if (State.isCurrentFp)
            {
                if(!State.isCursorActive)
                    RotateFP();
            }
            else
            {
                if (!State.isCursorActive)
                    RotateTP();
                RotateFPRoot();
            }
        }

        /// <summary> 1인칭 회전 </summary>
        private void RotateFP()
        {
            float deltaCoef = _deltaTime * 50f;

            // 상하 : FP Rig 회전
            float xRotPrev = Com.fpRig.localEulerAngles.x;
            float xRotNext = xRotPrev + _rotation.y
                * CamOption.rotationSpeed * deltaCoef;

            if (xRotNext > 180f)
                xRotNext -= 360f;

            // 좌우 : FP Root 회전
            float yRotPrev = Com.fpRoot.localEulerAngles.y;
            float yRotNext =
                yRotPrev + _rotation.x
                * CamOption.rotationSpeed * deltaCoef;

            // 상하 회전 가능 여부
            bool xRotatable =
                CamOption.lookUpDegree < xRotNext &&
                CamOption.lookDownDegree > xRotNext;

            // FP Rig 상하 회전 적용
            Com.fpRig.localEulerAngles = Vector3.right * (xRotatable ? xRotNext : xRotPrev);

            // FP Root 좌우 회전 적용
            Com.fpRoot.localEulerAngles = Vector3.up * yRotNext;
        }

        /// <summary> 3인칭 회전 </summary>
        private void RotateTP()
        {
            float deltaCoef = _deltaTime * 50f;

            // 상하 : TP Rig 회전
            float xRotPrev = Com.tpRig.localEulerAngles.x;
            float xRotNext = xRotPrev + _rotation.y
                * CamOption.rotationSpeed * deltaCoef;

            if (xRotNext > 180f)
                xRotNext -= 360f;

            // 좌우 : TP Rig 회전
            float yRotPrev = Com.tpRig.localEulerAngles.y;
            float yRotNext =
                yRotPrev + _rotation.x
                * CamOption.rotationSpeed * deltaCoef;

            // 상하 회전 가능 여부
            bool xRotatable =
                CamOption.lookUpDegree < xRotNext &&
                CamOption.lookDownDegree > xRotNext;

            Vector3 nextRot = new Vector3
            (
                xRotatable ? xRotNext : xRotPrev,
                yRotNext,
                0f
            );

            // TP Rig 회전 적용
            Com.tpRig.localEulerAngles = nextRot;
        }

        /// <summary> 3인칭일 경우 FP Root 회전 </summary>
        private void RotateFPRoot()
        {
            if (State.isMoving == false) return;

            Vector3 dir = Com.tpRig.TransformDirection(_moveDir);
            float currentY = Com.fpRoot.localEulerAngles.y;
            float nextY = Quaternion.LookRotation(dir, Vector3.up).eulerAngles.y;

            if (nextY - currentY > 180f) nextY -= 360f;
            else if (currentY - nextY > 180f) nextY += 360f;

            Com.fpRoot.eulerAngles = Vector3.up * Mathf.Lerp(currentY, nextY, 0.1f);
        }

        private void Move()
        {
            CheckForwardAndAdjustMoveVector();

            // 이동하지 않는 경우, 미끄럼 방지
            //if (State.isMoving == false)
            //{
            //    Com.rBody.velocity = new Vector3(0f, Com.rBody.velocity.y, 0f);
            //    return;
            //}

            // 실제 이동 벡터 계산
            // 1인칭
            if (State.isCurrentFp)
            {
                _worldMove = Com.fpRoot.TransformDirection(_moveDir);
            }
            // 3인칭
            else
            {
                _worldMove = Com.tpRig.TransformDirection(_moveDir);
            }

            //_currentSpeed = Mathf.Lerp(_currentSpeed, MoveOption.speed, 0.1f);

            _worldMove *= (MoveOption.speed) * (State.isRunning ? MoveOption.runningCoef : 1f);

            // Y축 속도는 유지하면서 XZ평면 이동
            Com.rBody.velocity = new Vector3(_worldMove.x, Com.rBody.velocity.y, _worldMove.z);
        }

        private void Jump()
        {
            if (!State.isGrounded) return;
            if (_currentJumpCooldown > 0f) return; // 점프 쿨타임

            if (Input.GetKeyDown(Key.jump))
            {
                Debug.Log("JUMP");

                // 하강 중 점프 시 속도가 합산되지 않도록 속도 초기화
                Com.rBody.velocity = Vector3.zero;

                Com.rBody.AddForce(Vector3.up * MoveOption.jumpForce, ForceMode.VelocityChange);

                // 애니메이션 점프 트리거
                Com.anim.SetTrigger(AnimOption.paramJump);

                // 쿨타임 초기화
                _currentJumpCooldown = MoveOption.jumpCooldown;
            }
        }

        private void ShowCursorToggle()
        {
            if (Input.GetKeyDown(Key.showCursor))
                State.isCursorActive = !State.isCursorActive;

            ShowCursor(State.isCursorActive);
        }

        private void ShowCursor(bool value)
        {
            Cursor.visible = value;
            Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;
        }

        private void CameraViewToggle()
        {
            if (Input.GetKeyDown(Key.switchCamera))
            {
                State.isCurrentFp = !State.isCurrentFp;
                Com.fpCamObject.SetActive(State.isCurrentFp);
                Com.tpCamObject.SetActive(!State.isCurrentFp);

                // TP -> FP
                if (State.isCurrentFp)
                {
                    Vector3 tpEulerAngle = Com.tpRig.localEulerAngles;
                    Com.fpRig.localEulerAngles = Vector3.right * tpEulerAngle.x;
                    Com.fpRoot.localEulerAngles = Vector3.up * tpEulerAngle.y;
                }
                // FP -> TP
                else
                {
                    Vector3 newRot = default;
                    newRot.x = Com.fpRig.localEulerAngles.x;
                    newRot.y = Com.fpRoot.localEulerAngles.y;
                    Com.tpRig.localEulerAngles = newRot;
                }
            }
        }

        private void TpCameraZoom()
        {
            if (State.isCurrentFp) return;                // TP 카메라만 가능
            if (Mathf.Abs(_currentWheel) < 0.01f) return; // 휠 입력 있어야 가능

            Transform tpCamTr = Com.tpCamera.transform;
            Transform tpCamRig = Com.tpRig;

            float zoom = _deltaTime * CamOption.zoomSpeed;
            float currentCamToRigDist = Vector3.Distance(tpCamTr.position, tpCamRig.position);
            Vector3 move = Vector3.forward * zoom * _currentWheel * 10f;

            // Zoom In
            if (_currentWheel > 0.01f)
            {
                if (_tpCamZoomInitialDistance - currentCamToRigDist < CamOption.zoomInDistance)
                {
                    tpCamTr.Translate(move, Space.Self);
                }
            }
            // Zoom Out
            else if (_currentWheel < -0.01f)
            {

                if (currentCamToRigDist - _tpCamZoomInitialDistance < CamOption.zoomOutDistance)
                {
                    tpCamTr.Translate(move, Space.Self);
                }
            }
        }

        private void UpdateAnimationParams()
        {
            float x, z;

            if (State.isCurrentFp)
            {
                x = _moveDir.x;
                z = _moveDir.z;

                if (State.isRunning)
                {
                    x *= 2f;
                    z *= 2f;
                }
            }
            else
            {
                x = 0f;
                z = _moveDir.sqrMagnitude > 0f ? 1f : 0f;

                if (State.isRunning)
                {
                    z *= 2f;
                }
            }

            // 보간
            const float LerpSpeed = 0.05f;
            _moveX = Mathf.Lerp(_moveX, x, LerpSpeed);
            _moveZ = Mathf.Lerp(_moveZ, z, LerpSpeed);

            Com.anim.SetFloat(AnimOption.paramMoveX, _moveX);
            Com.anim.SetFloat(AnimOption.paramMoveZ, _moveZ);
            Com.anim.SetFloat(AnimOption.paramDistY, _distFromGround);
            Com.anim.SetBool(AnimOption.paramGrounded, State.isGrounded);
        }

        private void UpdateCurrentValues()
        {
            if(_currentJumpCooldown > 0f)
                _currentJumpCooldown -= _deltaTime;
        }

        #endregion
    }
}