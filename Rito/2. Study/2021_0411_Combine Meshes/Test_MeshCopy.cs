using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-04-11 PM 7:57:31
// 작성자 : Rito

namespace Rito
{
    public class Test_MeshCopy : MonoBehaviour
    {
        public Material _material;
        public Transform[] _targets;

        private List<Vector3> _vertList = new List<Vector3>();
        private List<int> _triList = new List<int>();
        private List<Vector2> _uvList = new List<Vector2>();
        private List<Vector3> _normalList = new List<Vector3>();
        private List<Vector4> _tangentList = new List<Vector4>();

        private Mesh _mesh;
        private int _vertCount;

        private void Start()
        {
            transform.rotation = default;

            _mesh = _targets[0].GetComponent<MeshFilter>().mesh;
            _vertCount = _mesh.vertices.Length;

            ReleaseTargetParents();
            CreateMeshData();
            ApplyToMesh();
            DestroyTargets();
        }

        private void ReleaseTargetParents()
        {
            foreach (var target in _targets)
            {
                target.SetParent(null);
            }
        }

        private void CreateMeshData()
        {
            for (int i = 0; i < _targets.Length; i++)
            {
                Transform tr = _targets[i];
                Matrix4x4 mat = tr.localToWorldMatrix;
                Matrix4x4 mat2 = transform.worldToLocalMatrix;

                // 버텍스 공간변환하여 리스트에 추가
                foreach (var vert in _mesh.vertices)
                {
                    Vector3 vert1 = mat.MultiplyPoint(vert);
                    Vector3 vert2 = mat2.MultiplyPoint(vert1);
                    _vertList.Add(vert2);
                }

                // 삼각형 추가
                foreach (var tri in _mesh.triangles)
                {
                    _triList.Add(tri + (i * _vertCount));
                }

                // UV 추가
                _uvList.AddRange(_mesh.uv);

                // 노말, 탄젠트 공간변환하여 추가
                foreach (var normal in _mesh.normals)
                {
                    Vector3 normal1 = mat.MultiplyVector(normal);
                    _normalList.Add(normal1);
                }
                foreach (var tangent in _mesh.tangents)
                {
                    Vector3 tangent1 = mat.MultiplyVector(tangent);
                    _tangentList.Add(tangent1);
                }
            }
        }

        private void ApplyToMesh()
        {
            MeshFilter mf = gameObject.AddComponent<MeshFilter>();
            MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            mesh.name = _mesh.name.Replace(" Instance", "") + " (Combined)";

            mesh.vertices = _vertList.ToArray();
            mesh.triangles = _triList.ToArray();
            mesh.uv = _uvList.ToArray();

            mesh.normals = _normalList.ToArray();
            mesh.tangents = _tangentList.ToArray();
            mesh.RecalculateBounds();

            mr.material = _material;
            mf.mesh = mesh;
        }

        private void DestroyTargets()
        {
            for (int i = 0; i < _targets.Length; i++)
            {
                Destroy(_targets[i].gameObject);
            }
        }
    }
}