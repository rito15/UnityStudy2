using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 설명 : 
public class Test_CustomMesh : MonoBehaviour
{
    public float _radius = 2f;
    public int _resolution = 4; // 원 위의 꼭짓점 개수

    private MeshFilter _meshFilter;
    private Mesh _mesh;

    void Start()
    {
        CreateMesh();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateMesh();
        }
    }

    private void CreateMesh()
    {
        TryGetComponent(out _meshFilter);
        _mesh = new Mesh();
        _meshFilter.mesh = _mesh;

        CalculateMesh(out var verts, out var tris);

        _mesh.Clear();
        _mesh.vertices = verts;
        _mesh.triangles = tris;
        _mesh.RecalculateNormals();
    }

    private void CalculateMesh(out Vector3[] verts, out int[] tris)
    {
        Vector3 centerPoint = Vector3.zero; //transform.position;

        int vertsCount = _resolution + 1;
        int trisCount = _resolution * 3;

        verts = new Vector3[vertsCount];
        tris = new int[trisCount];

        Vector3 direction = Vector3.forward;
        Vector3 vertPoint = centerPoint + direction * _radius; // 원 위의 버텍스

        // 1. 버텍스 초기화
        verts[0] = centerPoint;
        for (int i = 1; i < vertsCount; i++)
        {
            verts[i] = vertPoint;

            // 회전하여 다음 버텍스 지점 찾기
            direction = Quaternion.Euler(0f, 360f / _resolution, 0f) * direction;
            vertPoint = centerPoint + direction * _radius;

            Debug.Log(vertPoint);
        }

        // 2. 트리스 초기화
        for (int i = 0; i < _resolution; i++)
        {
            tris[i * 3] = 0;
            tris[i * 3 + 1] = i + 1;
            tris[i * 3 + 2] = i + 2;
        }

        // 트리스 마지막 버텍스 오버플로 해결
        tris[trisCount - 1] = 1;
    }
}
