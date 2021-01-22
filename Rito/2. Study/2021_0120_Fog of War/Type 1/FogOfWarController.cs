using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-01-20 PM 5:17:35
// 작성자 : Rito

public class FogOfWarController : MonoBehaviour
{
    /***********************************************************************
    *                               Public Fields
    ***********************************************************************/
    #region .
    public int _resolution = 64;
    public float _width = 5f;
    public Material _fogMaterial;
    public Transform _fogCam;
    [Range(1f, 20f)]
    public float _sightRange = 5f;

    public List<Transform> _units = new List<Transform>(); // 안개 추적 대상

    [Space]
    public bool _showGizmo = true;
    public float _gizmoGap = 5f;

    #endregion
    /***********************************************************************
    *                               Private Fields
    ***********************************************************************/
    #region .
    private MeshFilter _filter;
    private MeshRenderer _renderer;
    private Mesh _mesh;

    private Vector3[] _verts;
    private int[] _tris;
    private Vector2 _gridUnit; // 한 그리드의 x, z 길이
    private Vector2Int _vCount;  // 각각 x, z의 버텍스 개수

    private bool[,] _visitedArray;
    private bool[,] _currentArray;

    private Color[] _colorArray;

    #endregion

    /***********************************************************************
    *                               Unity Events
    ***********************************************************************/
    #region .
    private void Awake()
    {
        Init();
        InitBlackColor();

        _visitedArray = new bool[_vCount.x, _vCount.y];
        _currentArray = new bool[_vCount.x, _vCount.y];
        _colorArray = new Color[_vCount.x * _vCount.y];

        StartCoroutine(UpdateFogAlphaRoutine());
    }

    IEnumerator UpdateFogAlphaRoutine()
    {
        while (true)
        {
            UpdateFogAlpha();

            yield return new WaitForSeconds(0.05f);
        }
    }

    private void UpdateFogAlpha()
    {
        // 1. Reset Current Array
        for (int i = 0; i < _vCount.x; i++)
            for (int j = 0; j < _vCount.y; j++)
            {
                _currentArray[i, j] = false;
            }

        // 2. Set Visited, Current Array
        foreach (var unit in _units)
        {
            Vector3 fogPosition = GetFogPosition(unit.position);
            (int x, int y) vIndex = GetVertexIndex(fogPosition);

            for (int i = 0; i < _vCount.x; i++)
                for (int j = 0; j < _vCount.y; j++)
                {
                    float x = Mathf.Abs(i - vIndex.x);
                    float y = Mathf.Abs(j - vIndex.y);

                    if (new Vector2(x, y).sqrMagnitude < _sightRange * _resolution)
                    {
                        _visitedArray[i, j] = true;
                        _currentArray[i, j] = true;
                    }
                }
        }

        // 3. Set Colors
        for (int i = 0; i < _vCount.x; i++)
            for (int j = 0; j < _vCount.y; j++)
            {
                int index = i + j * _vCount.x;

                bool visited = _visitedArray[i, j];
                bool current = _currentArray[i, j];

                float alpha;
                if (current) alpha = 0.0f;
                else if (visited) alpha = 0.5f;
                else alpha = 1.0f;

                _colorArray[index] = new Color(0, 0, 0, alpha);
            }

        _mesh.colors = _colorArray;
    }

    private void OnDrawGizmos()
    {
        Color[] colors = new[] { Color.red, Color.blue, Color.green };
        int i = 0;

        foreach (var unit in _units)
        {
            if (unit == null) continue;
            var fogPoint = GetFogPosition(unit.position);

            Gizmos.color = colors[i++];
            Gizmos.DrawWireSphere(fogPoint, 1f);
            Gizmos.DrawRay(_fogCam.position, unit.position - _fogCam.position);

            //GetVertexIndex(fogPoint);
            //Debug.Log(transform.InverseTransformPoint(fogPoint));
        }

        if (!_showGizmo) return;

        // Left Bottom Right Top
        Vector3 LB = transform.position - new Vector3(_width * 0.5f, 0f, _width * 0.5f);
        Vector3 LT = transform.position - new Vector3(_width * 0.5f, 0f, -_width * 0.5f);
        Vector3 RB = transform.position + new Vector3(_width * 0.5f, 0f, -_width * 0.5f);
        //Vector3 RT = transform.position + new Vector3(_width * 0.5f, 0f, _width * 0.5f);

        Gizmos.color = new Color(0, 0, 0, 0.5f);

        for (float x = 0; x <= _width; x += _gizmoGap)
        {
            Gizmos.DrawLine(LB + Vector3.right * x, LT + Vector3.right * x);
        }
        for (float z = 0; z <= _width; z += _gizmoGap)
        {
            Gizmos.DrawLine(LB + Vector3.forward * z, RB + Vector3.forward * z);
        }
    }

    #endregion
    /***********************************************************************
    *                               Methods
    ***********************************************************************/
    #region .
    private void Init()
    {
        TryGetComponent(out _filter);
        if (_filter == null)
            _filter = gameObject.AddComponent<MeshFilter>();

        TryGetComponent(out _renderer);
        if (_renderer == null)
            _renderer = gameObject.AddComponent<MeshRenderer>();

        GeneratePlane(out _verts, out _tris, out _gridUnit, out _vCount);

        _mesh = new Mesh();
        _mesh.vertices = _verts;
        _mesh.triangles = _tris;
        //_mesh.RecalculateNormals();

        _filter.mesh = _mesh;
        _renderer.material = _fogMaterial;
    }

    private void InitBlackColor()
    {
        Color[] colors = new Color[_mesh.vertexCount];

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color(0,0,0,1f);
        }

        _mesh.colors = colors;
    }

    /// <summary> 월드 포지션으로 포그 메시에 해당하는 버텍스 인덱스 구하기 </summary>
    private (int, int) GetVertexIndex(Vector3 worldPos)
    {
        Vector3 fogLocalPoint = transform.InverseTransformPoint(worldPos);
        Vector3 vertLocalPoint = fogLocalPoint + new Vector3(_width * 0.5f, 0f, _width * 0.5f);

        int vIndexX = Mathf.RoundToInt(vertLocalPoint.x / _gridUnit.x);
        int vIndexY = Mathf.RoundToInt(vertLocalPoint.z / _gridUnit.y);

        vIndexX = Math.Max(vIndexX, 0);
        vIndexY = Math.Max(vIndexY, 0);

        vIndexX = Mathf.Min(vIndexX, _vCount.x - 1);
        vIndexY = Mathf.Min(vIndexY, _vCount.y - 1);

        //Debug.Log($"{vIndexX}, {vIndexY}");

        return (vIndexX, vIndexY);
    }

    /// <summary> Input : 지상에 있는 유닛 위치 / Output : 카메라로 계산된 포그의 한 지점 </summary>
    private Vector3 GetFogPosition(Vector3 U)
    {
        Vector3 C = _fogCam.position;
        float Fy = transform.position.y;

        float Fx = U.x - (U.x - C.x) * Fy / C.y;
        float Fz = U.z - (U.z - C.z) * Fy / C.y;

        Vector3 fogPoint = new Vector3(Fx, Fy, Fz);

        return fogPoint;
    }

    #endregion
    /***********************************************************************
    *                               Mesh Generate
    ***********************************************************************/
    #region .
    private void GeneratePlane(out Vector3[] verts, out int[] tris, out Vector2 gridUnit, out Vector2Int vCount)
    {
        Vector3 widthV3 = new Vector3(_width, 0f, _width); // width를 3D로 변환
        Vector3 startPoint = -widthV3 * 0.5f;                   // 첫 버텍스의 위치
        gridUnit = Vector2.one * (_width / _resolution);               // 그리드 하나의 너비

        vCount = new Vector2Int(_resolution + 1, _resolution + 1); // 각각 가로, 세로 버텍스 개수
        int vertsCount = vCount.x * vCount.y;
        int trisCount = _resolution * _resolution * 6;

        verts = new Vector3[vertsCount];
        tris = new int[trisCount];

        // 1. 버텍스 초기화
        for (int j = 0; j < vCount.y; j++)
        {
            for (int i = 0; i < vCount.x; i++)
            {
                int index = i + j * vCount.x;
                verts[index] = startPoint
                    + new Vector3(
                        gridUnit.x * i,
                        0f,
                        gridUnit.y * j
                    );
            }
        }

        // 2. 트리스 초기화
        int tIndex = 0;
        for (int j = 0; j < vCount.y - 1; j++)
        {
            for (int i = 0; i < vCount.x - 1; i++)
            {
                int vIndex = i + j * vCount.x;

                tris[tIndex + 0] = vIndex;
                tris[tIndex + 1] = vIndex + vCount.x;
                tris[tIndex + 2] = vIndex + 1;

                tris[tIndex + 3] = vIndex + vCount.x;
                tris[tIndex + 4] = vIndex + vCount.x + 1;
                tris[tIndex + 5] = vIndex + 1;

                tIndex += 6;
            }
        }
    }

    #endregion
}