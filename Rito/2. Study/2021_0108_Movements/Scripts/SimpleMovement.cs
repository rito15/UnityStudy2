using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

* 리지드바디 기반 이동
  - 마우스 클릭 시, 클릭 위치에 도달할 때까지 이동
  - 마우스 이동 도중 키보드 이동 입력 시 마우스 이동 중지
    (우선순위 : 키보드 이동 > 마우스 이동)

* 조작 옵션
  - 키보드 이동 : WASD (설정 가능)
  - 마우스 이동 : RB (설정 가능)
  - 달리기 : Left Shift (설정 가능)

* 옵션
  - 속도
    - 이동 속도
    - 회전 속도
    - 달리기 속도 배수

  - 이동
    - 이동 기준 공간 : 월드 / 로컬
    - 키보드, 마우스 이동 기능 On/Off

  - 마우스
    - 마우스 클릭 인식 레이어마스크 설정
    - 마우스 클릭 지점 도착 판정 거리(threshold)

  - 애니메이터
    - Idle, Walk, Run 애니메이션 이름 스트링으로 설정

* 기능
  - 컴포넌트 추가 시,
    - 리지드바디가 존재하지 않을 경우 자동 추가
    - 콜라이더가 존재하지 않을 경우 캡슐 콜라이더 자동 추가 및 콜라이더 높이 자동 설정

[TODO]
* 마우스 휠업/다운 : 확대, 축소
* 화면 가장자리 카메라 이동 (On/Off 가능)

*/

namespace Rito
{
    public class SimpleMovement : MonoBehaviour
    {
        /***********************************************************************
         *                           Components
         ***********************************************************************/
        #region .
        private Rigidbody _rBody;
        private CapsuleCollider _capCol;
        private Animator _anim;

        public Rigidbody RBody => _rBody;
        public CapsuleCollider CapCol => _capCol;
        public Animator Anim => _anim;

        #endregion

        /***********************************************************************
         *                        Variables : Public
         ***********************************************************************/
        #region .
        [Serializable]
        public class SpeedOption
        {
            [Range(1f, 20f)]
            public float moveSpeed = 3f;
            [Range(1f, 20f)]
            public float turningSpeed = 2f;
            [Range(1f, 3f)]
            public float runSpeedMultiplier = 1.5f;
        }
        public SpeedOption _speedOption = new SpeedOption();

        [Serializable]
        public class MoveOption
        {
            /// <summary> 이동 기준 공간 </summary>
            public Space moveSpace = Space.World;
            public bool useKeyboardMove = true;
            public bool useMouseClickMove = true;
        }
        public MoveOption _moveOption = new MoveOption();

        [Serializable]
        public class KeyOption
        {
            public KeyCode moveForward = KeyCode.W;
            public KeyCode moveBackward = KeyCode.S;
            public KeyCode moveLeft = KeyCode.A;
            public KeyCode moveRight = KeyCode.D;
            public KeyCode run = KeyCode.LeftShift;
        }
        public KeyOption _keyOption = new KeyOption();

        [Serializable]
        public class MouseOption
        {
            public enum MoveButton
            {
                LeftButton, RightButton
            }

            public MoveButton moveButton = MoveButton.RightButton;
            public LayerMask clickLayerMask = -1;
            [Range(0.01f, 0.5f)]
            public float arrivalThreshold = 0.1f;
        }
        public MouseOption _mouseOption;


        [Serializable]
        public class AnimatorOption
        {
            public string idleAnimationName = "IDLE";
            public string walkAnimationName = "WALK";
            public string runAnimationName = "RUN";
        }
        public AnimatorOption _animatorOption = new AnimatorOption();

        #endregion

        /***********************************************************************
         *                        Variables : Private
         ***********************************************************************/
        #region .
        private float _characterHeight;
        private bool _isRunning = false;
        private Vector3 _keyMoveDir;
        private Vector3? _mouseMovePoint = null;

        #endregion

        /***********************************************************************
         *                           Unity Events
         ***********************************************************************/
        #region .
        protected virtual void Reset()
        {
            InitComponents();
            InitComponentSettings();
        }

        protected virtual void Awake()
        {
            TryGetComponent(out _rBody);
            TryGetComponent(out _capCol);
            TryGetComponentAndShowError(ref _anim, "애니메이터");
        }

        protected virtual void Update()
        {
            KeyMove();
            MouseMove();
            UpdateRunningState();
            UpdateAnimation();
        }

        #endregion

        /***********************************************************************
        *                           Movement Methods
        ***********************************************************************/
        #region .
        protected Vector3 CalculateKeyMoveDirection()
        {
            Vector3 moveDir = Vector3.zero;

            if (Input.GetKey(_keyOption.moveForward)) moveDir += Vector3.forward;
            if (Input.GetKey(_keyOption.moveBackward)) moveDir += Vector3.back;
            if (Input.GetKey(_keyOption.moveLeft)) moveDir += Vector3.left;
            if (Input.GetKey(_keyOption.moveRight)) moveDir += Vector3.right;

            return moveDir.normalized;
        }

        protected void KeyMove()
        {
            if (_moveOption.useKeyboardMove == false)
                return;

            Vector3 moveDir = CalculateKeyMoveDirection();

            if (moveDir.magnitude > 0.1f)
            {
                switch (_moveOption.moveSpace)
                {
                    case Space.World:
                        // Move
                        RBody.velocity = moveDir * _speedOption.moveSpeed * (_isRunning ? _speedOption.runSpeedMultiplier : 1f);

                        // Rotate
                        RotateToward(moveDir);
                        break;

                    case Space.Self:
                        // Move
                        Vector3 localMoveDir = transform.TransformDirection(Vector3.forward * moveDir.z).normalized;
                        RBody.velocity = localMoveDir * _speedOption.moveSpeed * (_isRunning ? _speedOption.runSpeedMultiplier : 1f);

                        // Rotate
                        transform.eulerAngles += new Vector3(0f, moveDir.x * _speedOption.turningSpeed * 2f, 0f);
                        break;

                    default:
                        RBody.velocity = default;
                        return;
                }

                _mouseMovePoint = null;
            }
            else
            {
                RBody.velocity = default;
            }
        }

        protected void MouseMove()
        {
            if (_moveOption.useMouseClickMove == false) return;
            if (_keyMoveDir.magnitude > 0.1f) return;

            // 마우스 이동 위치 설정
            if (Input.GetMouseButton((int)_mouseOption.moveButton))
            {
                bool success = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, 500f, _mouseOption.clickLayerMask);
                if (success)
                    _mouseMovePoint = hit.point;
            }

            // 자동 이동
            if (_mouseMovePoint != null)
            {
                // 도착 했다고 인식
                if (Vector3.Distance(transform.position, _mouseMovePoint.Value) < _mouseOption.arrivalThreshold)
                {
                    _mouseMovePoint = null;
                    return;
                }

                Vector3 moveVector = _mouseMovePoint.Value - transform.position;
                Vector3 moveDir = moveVector.normalized;

                // Move
                RBody.velocity = moveDir * _speedOption.moveSpeed * (_isRunning ? _speedOption.runSpeedMultiplier : 1f);

                // Rotate
                RotateToward(moveDir);
            }
        }

        protected void RotateToward(Vector3 dir)
        {
            Vector3 rotDir = new Vector3(dir.x, 0f, dir.z).normalized;
            transform.rotation = Quaternion.RotateTowards(
                          transform.rotation, Quaternion.LookRotation(rotDir), _speedOption.turningSpeed * Time.deltaTime * 200f);
        }

        protected void UpdateRunningState()
        {
            _isRunning = Input.GetKey(_keyOption.run);
        }

        protected void UpdateAnimation()
        {
            if (Anim == null)
                return;

            // IDLE
            if (RBody.velocity.magnitude < 0.1)
            {
                Anim.Play(_animatorOption.idleAnimationName);
            }
            // RUN
            else if (_isRunning)
            {
                Anim.Play(_animatorOption.runAnimationName);
            }
            // WALK
            else
            {
                Anim.Play(_animatorOption.walkAnimationName);
            }
        }

        #endregion

        /***********************************************************************
        *                             Init Methods
        ***********************************************************************/
        #region .

        /// <summary> 필요 컴포넌트 자동 추가 </summary>
        private void InitComponents()
        {
            var collider = GetComponentInChildren<Collider>();
            if (collider == null)
            {
                GetOrAddComponent(ref _capCol, "캡슐 콜라이더");
            }
            GetOrAddComponent(ref _rBody, "리지드바디");
            TryGetComponentAndShowError(ref _anim, "애니메이터");
        }

        private void InitComponentSettings()
        {
            // 1. 캡슐 콜라이더 영역 설정
            if (CapCol != null)
            {
                _characterHeight = CalculateCharacterHeight(); // 캐릭터 높이 구하기
                CapCol.height = _characterHeight;
                CapCol.center = new Vector3(0f, _characterHeight * 0.5f, 0f);
            }

            // 2. 리지드바디 설정
            if (RBody != null)
            {
                RBody.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }

        #endregion

        /***********************************************************************
         *                           Private Methods
         ***********************************************************************/
        #region .
        private float CalculateCharacterHeight()
        {
            var smr = GetComponentInChildren<SkinnedMeshRenderer>();
            Mesh mesh;

            if (smr != null)
            {
                mesh = smr.sharedMesh;
            }
            else
            {
                Debug.Log("캐릭터에 Skinned Mesh Renderer가 없습니다");
                var mFilter = GetComponentInChildren<MeshFilter>();

                if (mFilter == null)
                {
                    Debug.Log("캐릭터에 메시가 존재하지 않습니다");
                    return 1f;
                }

                mesh = mFilter.sharedMesh;
            }

            float max = float.MinValue;
            float min = float.MaxValue;

            foreach (var vertex in mesh.vertices)
            {
                if (vertex.y > max) max = vertex.y;
                if (vertex.y < min) min = vertex.y;
            }

            return (max - min);
        }

        private void GetOrAddComponent<T>(ref T component, in string componentName) where T : Component
        {
            component = GetComponentInChildren<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
                Debug.Log($"{componentName} 자동 추가 완료");
            }
        }

        private void TryGetComponentAndShowError<T>(ref T component, in string componentName) where T : Component
        {
            if (component != null) return;

            component = GetComponentInChildren<T>();
            if (component == null)
            {
                Debug.LogError($"{componentName}을(를) 등록해주세요");
            }
        }

        #endregion
    }
}