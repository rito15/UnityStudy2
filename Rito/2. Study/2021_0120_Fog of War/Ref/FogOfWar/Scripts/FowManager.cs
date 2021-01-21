using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.FogOfWar
{
    [DefaultExecutionOrder(-100)]
    public class FowManager : MonoBehaviour
    {
        #region Singleton - Public

        /// <summary> 싱글톤 인스턴스 Getter </summary>
        public static FowManager Instance
        {
            get
            {
                if (_instance == null)    // 체크 1 : 인스턴스가 없는 경우
                    CheckExsistence();

                return _instance;
            }
        }
        /// <summary> 싱글톤 인스턴스의 또다른 이름 </summary>
        public static FowManager I => Instance;

        #endregion // ==================================================================

        #region Singleton - Private

        // 싱글톤 인스턴스
        private static FowManager _instance;

        // 싱글톤 인스턴스 존재 여부 확인 (체크 2)
        private static void CheckExsistence()
        {
            // 싱글톤 검색
            _instance = FindObjectOfType<FowManager>();

            // 인스턴스 가진 오브젝트가 존재하지 않을 경우, 빈 오브젝트를 임의로 생성하여 인스턴스 할당
            if (_instance == null)
            {
                // 빈 게임 오브젝트 생성
                GameObject container = new GameObject("FowManager Singleton Container");

                // 게임 오브젝트에 클래스 컴포넌트 추가 후 인스턴스 할당
                _instance = container.AddComponent<FowManager>();
            }
        }

        /// <summary> 
        /// [Awake()에서 호출]
        /// <para/> 싱글톤 스크립트를 미리 오브젝트에 담아 사용하는 경우를 위한 로직
        /// </summary>
        private void CheckInstance()
        {
            // 싱글톤 인스턴스가 존재하지 않았을 경우, 본인으로 초기화
            if (_instance == null)
                _instance = this;

            // 싱글톤 인스턴스가 존재하는데, 본인이 아닐 경우, 스스로(컴포넌트)를 파괴
            if (_instance != null && _instance != this)
            {
                Debug.Log("이미 FowManager 싱글톤이 존재하므로 오브젝트를 파괴합니다.");
                Destroy(this);

                // 만약 게임 오브젝트에 컴포넌트가 자신만 있었다면, 게임 오브젝트도 파괴
                var components = gameObject.GetComponents<Component>();

                if (components.Length <= 2)
                    Destroy(gameObject);
            }
        }

        #endregion // ==================================================================

        /***********************************************************************
        *                               Fields
        ***********************************************************************/
        #region .

        public float _fogWidthX = 20;
        public float _fogWidthZ = 20;
        public float _tileSize = 1; // 타일 하나의 크기
        public float _updateCycle = 0.5f;

        public bool _showGizmos = true;

        public FowMap Map { get; private set; }
        private int[,] MapData { get; set; }
        private List<FowUnit> UnitList { get; set; } // Fow 시스템이 추적할 유닛들

        #endregion
        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .
        private void Awake()
        {
            CheckInstance();
            UnitList = new List<FowUnit>();
            InitMap();
        }
        private void OnEnable()
        {
            StartCoroutine(UpdateFogRoutine());
        }

        private void Update()
        {
            Map.Lerp();
        }

        private void OnDestroy()
        {
            Map.Release();
        }
        #endregion

        /***********************************************************************
        *                               Static Methods (Add/Remove)
        ***********************************************************************/
        #region .
        public static void AddUnit(FowUnit unit)
        {
            if (!_instance.UnitList.Contains(unit))
            {
                _instance.UnitList.Add(unit);
            }
        }
        public static void RemoveUnit(FowUnit viewer)
        {
            if (_instance.UnitList.Contains(viewer))
            {
                _instance.UnitList.Remove(viewer);
            }
        }

        #endregion
        /***********************************************************************
        *                               Private Methods
        ***********************************************************************/
        #region .
        public void InitMap()
        {
            MapData = new int[(int)(_fogWidthX / _tileSize), (int)(_fogWidthZ / _tileSize)];
            for (int i = 0; i < MapData.GetLength(0); i++)
            {
                for (int j = 0; j < MapData.GetLength(1); j++)
                {
                    if (Physics.CheckBox(
                        GetTileCenterPoint(new TilePos(i, j)),
                        new Vector3(_tileSize - 0.02f, 0f, _tileSize - 0.02f) * 0.5f))
                    {
                        MapData[i, j] = 1;
                    }
                    else
                    {
                        MapData[i, j] = 0;
                    }
                }
            }

            Map = new FowMap();
            Map.InitMap(MapData);
        }

        /// <summary> 대상 유닛의 위치를 타일좌표(x 인덱스, y 인덱스)로 환산 </summary>
        private TilePos GetTilePos(FowUnit viewer)
        {
            var x = (int)((viewer.transform.position.x - transform.position.x + _fogWidthX * 0.5f) / _tileSize);
            var y = (int)((viewer.transform.position.z - transform.position.z + _fogWidthZ * 0.5f) / _tileSize);

            return new TilePos(x, y);
        }

        /// <summary> 해당 (x,y) 인덱스의 타일 중심 좌표 구하기 </summary>
        private Vector3 GetTileCenterPoint(in TilePos pos)
        {
            return transform.position
                + new Vector3(pos.x * _tileSize, 0, pos.y * _tileSize)
                + new Vector3(_tileSize * 0.5f, 0, _tileSize * 0.5f)
                - new Vector3(_fogWidthX * 0.5f, 0, _fogWidthZ * 0.5f);
        }

        #endregion
        /***********************************************************************
        *                               Coroutine
        ***********************************************************************/
        #region .
        public IEnumerator UpdateFogRoutine()
        {
            while (true)
            {
                if (Map != null)
                {
                    Map.RefreshFog();

                    foreach (var viewer in UnitList)
                    {
                        TilePos pos = GetTilePos(viewer);
                        Map.ComputeFog(pos, viewer.sightRange / _tileSize);
                    }
                }

                yield return new WaitForSeconds(_updateCycle);
            }
        }

        #endregion

        /***********************************************************************
        *                               Editor
        ***********************************************************************/
        #region .

        private void OnDrawGizmos()
        {
            if (Application.isPlaying == false) return;
            if (_showGizmos == false) return;

            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position, new Vector3(_fogWidthX, 0f, _fogWidthZ));
            if (MapData != null)
            {
                for (int i = 0; i < MapData.GetLength(0); i++)
                {
                    for (int j = 0; j < MapData.GetLength(1); j++)
                    {
                        Gizmos.color = MapData[i, j] == 1 ? Color.red : Color.white;
                        Gizmos.DrawWireCube(GetTileCenterPoint(new TilePos(i, j)), new Vector3(_tileSize - 0.02f, 0f, _tileSize - 0.02f));
                    }
                }
                foreach (var unit in UnitList)
                {
                    TilePos pos = GetTilePos(unit);
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(GetTileCenterPoint(pos), new Vector3(_tileSize - 0.02f, 1f, _tileSize - 0.02f));

                }
            }
        }

        #endregion
    }
}
