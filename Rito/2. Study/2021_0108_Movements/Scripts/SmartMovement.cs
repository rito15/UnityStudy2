using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-01-12 PM 4:06:23
// 작성자 : Rito

/*
    [하이라키 구성]

    Root

    ㄴ Walker (SmartWalker 컴포넌트)
        ㄴ FP Rig
            ㄴFP Cam (FirstPersonCamera 컴포넌트)

        ㄴ ★캐릭터 모델링

    ㄴ TP Rig
        ㄴ TP Cam (ThirdPersonCamera 컴포넌트)

*/

public class SmartMovement : MonoBehaviour
{
    /***********************************************************************
    *                           Unity Events
    ***********************************************************************/
    #region .
    private void Awake()
    {
        InitializeComponents();
        InitializeValues();
    }

    private void Start()
    {
        StartCoroutine(CameraZoomRoutine());
    }

    private void Update()
    {
        Input_ChangeCamView();
        Input_SetCursorVisibleState();
        Input_RotatePlayer();
        Input_CameraZoom();
        Input_CalculateKeyMoveDir();

        MoveByKeyboard();
        Input_MoveKeyUpBrake();

        if (Anim != null)
        {
            if (PlayerIsWalking())
            {
                PlayWalkAnimation();
            }
            else if (PlayerIsRunning())
            {
                PlayRunAnimation();
            }
            else
            {
                PlayIdleAnimation();
            }
        }
    }
    #endregion


    /***********************************************************************
    *                           Enum Definitions
    ***********************************************************************/
    #region .
    public enum CameraViewOptions
    {
        FirstPerson,
        ThirdPerson
    }

    #endregion


    /***********************************************************************
    *                           Properties
    ***********************************************************************/
    #region .
    public KeyOption Key => _keyOption;
    public MoveOptionInfo Move => _moveOption;
    public AnimationNameInfo AnimationName => _animationName;

    public CameraOptionFirstPerson FPCamOption => _firstPersonCameraOption;
    public CameraOptionThirdPerson TPCamOption => _thirdPersonCameraOption;

    public Transform Walker { get; private set; }

    public Rigidbody RBody
    { get; private set; }
    public Animator Anim { get; private set; }

    public FirstPersonCamera FPCam { get; private set; }
    public ThirdPersonCamera TPCam { get; private set; }

    #endregion
    /***********************************************************************
    *                           States
    ***********************************************************************/
    #region .
    [Serializable]
    public class PlayerState
    {
        public bool isDead;

        /// <summary> 캐릭터가 걷거나 뛰고 있는지 여부 </summary>
        public bool isMoving;

        /// <summary> 캐릭터가 걷고 있는지 여부 </summary>
        public bool isWalking;

        /// <summary> 캐릭터가 뛰는고 있는지 여부 </summary>
        public bool isRunning;

        /// <summary> 현재 커서가 보이는지 여부 </summary>
        public bool isCursorVisible;

        /// <summary> 현재 선택된 카메라 뷰 </summary>
        public CameraViewOptions currentView = CameraViewOptions.ThirdPerson;
    }
    [Space, SerializeField]
    public PlayerState _state = new PlayerState();
    public PlayerState State => _state;

    #endregion
    /***********************************************************************
    *                           Variables
    ***********************************************************************/
    #region .
    /// <summary> WASD 이동 벡터 </summary>
    private Vector3 _moveDir;

    /// <summary> TP 카메라 -> Rig 방향 벡터 </summary>
    private Vector3 _tpCamToRigDir;

    /// <summary> TP 카메라 ~ Rig 초기 거리 </summary>
    private float _tpCamZoomInitialDistance;

    /// <summary> TP 카메라 휠 입력 값 </summary>
    private float _tpCameraWheelInput = 0;

    private bool _isMouseMiddlePressed = false;
    private bool _prevCursorVisibleState = false;


    /// <summary> 현재 선택된 카메라 </summary>
    private PersonalCamera _currentCam;
    /// <summary> 현재 선택된 카메라의 옵션 </summary>
    private CameraOption _currentCamOption;

    #endregion
    /***********************************************************************
    *                           Animations
    ***********************************************************************/
    #region .
    [Serializable]
    public class AnimationNameInfo
    {
        public string idle = "IDLE";
        public string walk = "WALK";
        public string run = "RUN";
    }
    [SerializeField, Tooltip("연결된 애니메이터의 각 애니메이션 이름 정확히 등록")]
    private AnimationNameInfo _animationName = new AnimationNameInfo();

    #endregion
    /***********************************************************************
    *                           Key Options
    ***********************************************************************/
    #region .
    [Serializable]
    public class KeyOption
    {
        public KeyCode moveForward = KeyCode.W;
        public KeyCode moveBackward = KeyCode.S;
        public KeyCode moveLeft = KeyCode.A;
        public KeyCode moveRight = KeyCode.D;

        public KeyCode run = KeyCode.LeftShift;

        [Tooltip("마우스 커서 보이기/감추기 토글")]
        public KeyCode showCursorToggle = KeyCode.LeftAlt;

        [Tooltip("1인칭 / 3인칭 카메라 변경 토글")]
        public KeyCode changeViewToggle = KeyCode.Tab;
    }
    [SerializeField]
    private KeyOption _keyOption = new KeyOption();

    #endregion
    /***********************************************************************
    *                           Move Options
    ***********************************************************************/
    #region .
    [Serializable]
    public class MoveOptionInfo
    {
        [Range(1f, 20f), Tooltip("캐릭터 이동속도")]
        public float moveSpeed = 3f;
        [Range(1f, 3f), Tooltip("달리기 이동속도 배수(달리기 이동속도 = 캐릭터 이동속도 X 달리기 배수)")]
        public float runSpeedMultiplier = 1.5f;
    }
    [SerializeField]
    private MoveOptionInfo _moveOption = new MoveOptionInfo();

    #endregion
    /***********************************************************************
    *                           Camera Options
    ***********************************************************************/
    #region .
    // 상속용
    public abstract class CameraOption
    {
        [Range(1f, 20f), Space, Tooltip("카메라 상하좌우 회전 속도")]
        public float rotationSpeed = 2f;
        [Range(-90f, 0f), Tooltip("올려다보기 제한 각도")]
        public float lookUpDegree = -60f;
        [Range(0f, 60f), Tooltip("내려다보기 제한 각도")]
        public float lookDownDegree = 45f;
    }

    [Serializable]
    public class CameraOptionFirstPerson : CameraOption
    {
    }
    [SerializeField]
    private CameraOptionFirstPerson _firstPersonCameraOption = new CameraOptionFirstPerson();

    [Serializable]
    public class CameraOptionThirdPerson : CameraOption
    {
        [Range(0f, 3.5f), Space, Tooltip("줌 확대 최대 거리")]
        public float zoomInDistance = 3f;
        [Range(0f, 5f), Tooltip("줌 축소 최대 거리")]
        public float zoomOutDistance = 3f;
        [Range(1f, 10f), Tooltip("줌 속도")]
        public float zoomSpeed = 5f;
    }
    [SerializeField]
    private CameraOptionThirdPerson _thirdPersonCameraOption = new CameraOptionThirdPerson();

    #endregion


    /***********************************************************************
    *                           Init Methods
    ***********************************************************************/
    #region .
    private void InitializeComponents()
    {
        // Gets
        SmartWalker walker = GetComponentInChildren<SmartWalker>();
        Walker = walker.transform;

        RBody = GetComponent<Rigidbody>();
        Anim = GetComponentInChildren<Animator>();

        FPCam = GetComponentInAllChildren<FirstPersonCamera>();
        TPCam = GetComponentInAllChildren<ThirdPersonCamera>();
        FPCam.Init();
        TPCam.Init();

        // Error Check
        if (RBody == null) Debug.LogError("플레이어 캐릭터에 리지드바디가 존재하지 않습니다.");
        if (Anim == null) Debug.LogError("플레이어 캐릭터에 애니메이터가 존재하지 않습니다.");

        // Init Component Values
        RBody.constraints = RigidbodyConstraints.FreezeRotation;
        Anim.applyRootMotion = false;
    }

    private void InitializeValues()
    {
        // Cursor
        SetCursorVisibleState(false);

        // Camera
        Vector3 camToRig = TPCam.Rig.position - TPCam.transform.position;
        _tpCamToRigDir = TPCam.transform.InverseTransformDirection(camToRig).normalized;
        _tpCamZoomInitialDistance = Vector3.Magnitude(camToRig);

        SetCameraView(State.currentView); // 초기 뷰 설정
        SetCameraAlone(); // 카메라 한개 빼고 전부 비활성화
    }

    #endregion
    /***********************************************************************
    *                           Setter Methods
    ***********************************************************************/
    #region .

    private void SetCursorVisibleState(bool value)
    {
        if (value)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void SetCameraView(CameraViewOptions view)
    {
        State.currentView = view;
        bool isFP = view == CameraViewOptions.FirstPerson;

        FPCam.Cam.gameObject.SetActive(isFP);
        TPCam.Cam.gameObject.SetActive(!isFP);

        if (isFP)
        {
            _currentCam = FPCam;
            _currentCamOption = FPCamOption;
        }
        else
        {
            _currentCam = TPCam;
            _currentCamOption = TPCamOption;
        }
    }

    /// <summary> 현재 활성화된 주요 카메라 외에 모든 카메라 게임오브젝트 비활성화 </summary>
    private void SetCameraAlone()
    {
        var cams = FindObjectsOfType<Camera>();
        foreach (var cam in cams)
        {
            if (cam != _currentCam.Cam)
            {
                cam.gameObject.SetActive(false);
            }
        }
    }

    #endregion
    /***********************************************************************
    *                           Toggle Methods
    ***********************************************************************/
    #region .
    private void ToggleCameraView()
    {
        SetCameraView(State.currentView == CameraViewOptions.FirstPerson ?
            CameraViewOptions.ThirdPerson : CameraViewOptions.FirstPerson);
    }

    #endregion

    /***********************************************************************
    *                           Checker Methods
    ***********************************************************************/
    #region .
    private bool PlayerIsWalking() => State.isWalking;

    private bool PlayerIsRunning() => State.isRunning;

    private bool CurrentIsFPCamera() => State.currentView == CameraViewOptions.FirstPerson;
    private bool CurrentIsTPCamera() => State.currentView == CameraViewOptions.ThirdPerson;

    #endregion
    /***********************************************************************
    *                           Calculation Methods
    ***********************************************************************/
    #region .


    #endregion
    /***********************************************************************
    *                           Finder Methods
    ***********************************************************************/
    #region .
    /// <summary> Active False인 자식도 다 뒤져서 컴포넌트 찾아오기 </summary>
    private T GetComponentInAllChildren<T>() where T : Component
    {
        List<Transform> _childrenTrList = new List<Transform>();
        Recur_GetAllChildrenTransform(_childrenTrList, transform);

        foreach (var tr in _childrenTrList)
        {
            T found = tr.GetComponent<T>();
            if (found != null)
                return found;
        }
        return null;
    }
    private void Recur_GetAllChildrenTransform(List<Transform> trList, Transform tr)
    {
        trList.Add(tr);
        int childCount = tr.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Recur_GetAllChildrenTransform(trList, tr.GetChild(i));
        }
    }

    #endregion
    /***********************************************************************
    *                           Animation Player Methods
    ***********************************************************************/
    #region .

    private void PlayIdleAnimation() => Anim.Play(AnimationName.idle);
    private void PlayWalkAnimation() => Anim.Play(AnimationName.walk);
    private void PlayRunAnimation() => Anim.Play(AnimationName.run);

    #endregion
    /***********************************************************************
    *                           Player Action Methods
    ***********************************************************************/
    #region .
    /// <summary> 키보드 WASD 이동 </summary>
    private void MoveByKeyboard()
    {
        if (!State.isMoving) return;

        Vector3 worldMoveDir;
        if (CurrentIsTPCamera())
        {
            worldMoveDir = TPCam.Rig.TransformDirection(_moveDir);
        }
        else
        {
            worldMoveDir = Walker.TransformDirection(_moveDir);
        }

        Vector3 next = worldMoveDir
            * Move.moveSpeed * (State.isRunning ? Move.runSpeedMultiplier : 1f);

        RBody.velocity = new Vector3(next.x, RBody.velocity.y, next.z);

        // 워커 회전
        if (CurrentIsTPCamera())
        {
            Vector3 dir = TPCam.Rig.TransformDirection(_moveDir);
            float currentY = Walker.localEulerAngles.y;
            float nextY = Quaternion.LookRotation(dir, Vector3.up).eulerAngles.y;

            if (nextY - currentY > 180f) nextY -= 360f;
            else if (currentY - nextY > 180f) nextY += 360f;

            Walker.eulerAngles = Vector3.up * Mathf.Lerp(currentY, nextY, 0.05f);
        }
    }

    #endregion
    /***********************************************************************
    *                           Input Action Methods
    ***********************************************************************/
    #region .

    /// <summary> WASD 키를 떼면 XZ 이동 브레이크 </summary>
    private void Input_MoveKeyUpBrake()
    {
        if (Input.GetKeyUp(Key.moveForward) ||
            Input.GetKeyUp(Key.moveBackward) ||
            Input.GetKeyUp(Key.moveLeft) ||
            Input.GetKeyUp(Key.moveRight))
        {
            RBody.velocity = new Vector3(0f, RBody.velocity.y, 0f);
        }
    }

    /// <summary> FP, TP 카메라 변경 </summary>
    private void Input_ChangeCamView()
    {
        if (Input.GetKeyDown(Key.changeViewToggle))
        {
            ToggleCameraView();
        }
    }

    /// <summary> 키보드 Alt, 마우스 MB 입력으로 커서 보이기/감추기 </summary>
    private void Input_SetCursorVisibleState()
    {
        // 1. 마우스 중앙버튼 유지하는 동안 커서 감추기
        if (Input.GetMouseButtonDown(2))
        {
            _isMouseMiddlePressed = true;
            _prevCursorVisibleState = State.isCursorVisible;

            if (State.isCursorVisible)
            {
                SetCursorVisibleState(false);
            }
        }
        if (Input.GetMouseButtonUp(2))
        {
            _isMouseMiddlePressed = false;

            if (_prevCursorVisibleState)
            {
                SetCursorVisibleState(true);
            }
        }

        // 2. Alt 눌러 커서 토글
        if (!_isMouseMiddlePressed && Input.GetKeyDown(Key.showCursorToggle))
        {
            State.isCursorVisible = !State.isCursorVisible;
            SetCursorVisibleState(State.isCursorVisible);
        }
    }

    /// <summary> 마우스를 상하/좌우로 움직여서 카메라 회전 </summary>
    private void Input_RotatePlayer()
    {
        if (State.currentView == CameraViewOptions.FirstPerson) RotateFP();
        else RotateTP();
    }

    private void RotateFP()
    {
        Transform fpCamRig = FPCam.Rig; // Rig : 상하 회전
        Transform walker = Walker;  // Walker : 좌우 회전

        // ================================================
        // 상하 : 카메라 Rig 회전
        float vDegree = -Input.GetAxisRaw("Mouse Y");
        float xRotPrev = fpCamRig.localEulerAngles.x;
        float xRotNext = xRotPrev
            + vDegree
            * _currentCamOption.rotationSpeed
            * Time.deltaTime * 50f;

        if (xRotNext > 180f)
            xRotNext -= 360f;

        // ================================================
        // 좌우 : 워커 회전
        float hDegree = Input.GetAxisRaw("Mouse X");
        float yRotPrev = walker.localEulerAngles.y;
        float yRotAdd =
            hDegree
            * _currentCamOption.rotationSpeed
            * Time.deltaTime * 50f;
        float yRotNext = yRotAdd + yRotPrev;

        // 상하 회전 가능 여부
        bool xRotatable =
            _currentCamOption.lookUpDegree < xRotNext &&
            _currentCamOption.lookDownDegree > xRotNext;

        // Rig 상하 회전 적용
        fpCamRig.localEulerAngles = Vector3.right * (xRotatable ? xRotNext : xRotPrev);

        // 워커 좌우 회전 적용
        walker.localEulerAngles = Vector3.up * yRotNext;
    }

    float cur;
    private void RotateTP()
    {
        if (State.isCursorVisible && !_isMouseMiddlePressed)
            return;

        Transform tpCamRig = TPCam.Rig;

        // ================================================
        // 상하 : 카메라 Rig 회전
        float vDegree = -Input.GetAxisRaw("Mouse Y");
        float xRotPrev = tpCamRig.localEulerAngles.x;
        float xRotNext = xRotPrev
            + vDegree
            * _currentCamOption.rotationSpeed
            * Time.deltaTime * 50f;

        if (xRotNext > 180f)
            xRotNext -= 360f;

        // ================================================
        // 좌우 : 카메라 Rig 회전
        float hDegree = Input.GetAxisRaw("Mouse X");
        float yRotPrev = tpCamRig.localEulerAngles.y;

        float yRotAdd =
            hDegree
            * _currentCamOption.rotationSpeed
            * Time.deltaTime * 50f;
        float yRotNext = yRotAdd + yRotPrev;

        // 상하 회전 가능 여부 판정
        bool xRotatable =
            _currentCamOption.lookUpDegree < xRotNext &&
            _currentCamOption.lookDownDegree > xRotNext;

        Vector3 nextRot = new Vector3
        (
            xRotatable ? xRotNext : xRotPrev,
            yRotNext,
            0f
        );

        // Rig 상하좌우 회전 적용
        tpCamRig.localEulerAngles = nextRot;
    }


    /// <summary> TP Cam : 마우스 휠 굴려서 확대/축소 </summary>
    private void Input_CameraZoom()
    {
        if (State.currentView == CameraViewOptions.FirstPerson)
            return;

        _tpCameraWheelInput = Input.GetAxis("Mouse ScrollWheel");
    }

    /// <summary> WASD, LShift 입력으로 이동 벡터, 이동 상태 정의 </summary>
    private void Input_CalculateKeyMoveDir()
    {
        _moveDir = Vector3.zero;

        if (Input.GetKey(Key.moveForward)) _moveDir += Vector3.forward;
        if (Input.GetKey(Key.moveBackward)) _moveDir += Vector3.back;
        if (Input.GetKey(Key.moveLeft)) _moveDir += Vector3.left;
        if (Input.GetKey(Key.moveRight)) _moveDir += Vector3.right;

        bool isRunningKeyDown = Input.GetKey(Key.run);
        bool moving = _moveDir.magnitude > 0.1f;

        State.isMoving = moving;
        State.isWalking = moving && !isRunningKeyDown;
        State.isRunning = moving && isRunningKeyDown;
    }

    #endregion


    /***********************************************************************
    *                           Coroutines
    ***********************************************************************/
    #region .
    private IEnumerator CameraZoomRoutine()
    {
        var wfs = new WaitForSeconds(0.005f);

        Transform tpCamTr = TPCam.transform;
        Transform tpCamRig = TPCam.Rig;

        while (true)
        {
            // Zoom In
            if (_tpCameraWheelInput > 0.01f)
            {
                for (float f = 0f; f < 0.3f;)
                {
                    // Zoom In 도중 Zoom Out 명령 받으면 종료
                    if (_tpCameraWheelInput < -0.01f) break;

                    float deltaTime = Time.deltaTime;
                    float zoom = deltaTime * TPCamOption.zoomSpeed;
                    float currentCamToRigDist = Vector3.Distance(tpCamTr.position, tpCamRig.position);

                    if (_tpCamZoomInitialDistance - currentCamToRigDist < TPCamOption.zoomInDistance)
                    {
                        tpCamTr.Translate(_tpCamToRigDir * zoom, Space.Self);
                    }

                    f += deltaTime;
                    yield return null;
                }
            }
            // Zoom Out
            else if (_tpCameraWheelInput < -0.01f)
            {
                for (float f = 0f; f < 0.3f;)
                {
                    // Zoom Out 도중 Zoom In 명령 받으면 종료
                    if (_tpCameraWheelInput > 0.01f) break;

                    float deltaTime = Time.deltaTime;
                    float zoom = deltaTime * TPCamOption.zoomSpeed;
                    float currentCamToRigDist = Vector3.Distance(tpCamTr.position, tpCamRig.position);

                    if (currentCamToRigDist - _tpCamZoomInitialDistance < TPCamOption.zoomOutDistance)
                    {
                        tpCamTr.Translate(-_tpCamToRigDir * zoom, Space.Self);
                    }

                    f += deltaTime;
                    yield return null;
                }
            }

            yield return wfs;
        }
    }

    #endregion
}