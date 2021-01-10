using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-01-10 PM 4:44:56
// 작성자 : Rito

namespace Rito.MeshGenerator
{
    public class SnowGroundMeshGenerator : PerlinNoiseMeshGenerator
    {
        public bool _allowFootPrint = false;
        public float _footPrintDepth = 0.1f;

        protected Vector3[] _originVerts;

        public override void GenerateMesh()
        {
            base.GenerateMesh();

            var meshCol = GetComponent<MeshCollider>();
            if (meshCol != null)
                DestroyImmediate(meshCol);
            gameObject.AddComponent<MeshCollider>();
        }

        protected override void Awake()
        {
            base.Awake();
            _originVerts = new Vector3[_verts.Length];
            Array.Copy(_verts, _originVerts, 0); // 발자국 남기기 전의 버텍스 목록 카피
        }

        /// <summary> 해당 위치에 가장 근접한 버텍스 인덱스 찾기 </summary>
        private int FindVertexIndex(Vector3 pos)
        {
            // 좌측 하단 지점
            Vector2 zeroPoint = new Vector2(
                transform.position.x - _width.x * 0.5f,
                transform.position.z - _width.y * 0.5f
            );
            // 우측 상단 지점
            Vector2 onePoint = new Vector2(
                transform.position.x + _width.x * 0.5f,
                transform.position.z + _width.y * 0.5f
            );

            // 메시 영역 밖인 경우 예외처리
            if (pos.x < zeroPoint.x || pos.x > onePoint.x ||
                pos.z < zeroPoint.y || pos.z > onePoint.y)
                return -1;

            // ========================================================
            Vector2Int vertCounts = new Vector2Int(_resolution.x + 1, _resolution.y + 1);

            // 메시 영역 내에서 지점의 xy비율(0 ~ 1)
            Vector2 ratio = new Vector3(
                (pos.x - zeroPoint.x) / _width.x,
                (pos.z - zeroPoint.y) / _width.y
            );

            Vector2Int vertsIndex = new Vector2Int((int)(ratio.x * vertCounts.x), (int)(ratio.y * vertCounts.y));

            return vertsIndex.x + vertsIndex.y * vertCounts.x;
        }

        private Vector3 GetVertexWorldPoisition(int vertIndex)
        {
            return _verts[vertIndex] + transform.position;
        }

        private Vector3 GetVertexLocalPoisition(int vertIndex)
        {
            return _verts[vertIndex];
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_allowFootPrint == false)
                return;

            // 충돌 지점마다 높이 깎아서 발자국 만들기
            foreach (var contact in collision.contacts)
            {
                int vIndex = FindVertexIndex(contact.point);
                if (vIndex < 0)
                    continue;

                Vector3 vLocalPos = GetVertexLocalPoisition(vIndex);
                float snowPrintedHeight = _originVerts[vIndex].y - _footPrintDepth;

                if (vLocalPos.y > snowPrintedHeight)
                    _verts[vIndex] = new Vector3(vLocalPos.x, snowPrintedHeight, vLocalPos.z);
            }
            _mesh.vertices = _verts;
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();

            //Debug.Log("Collision");

            //int vertIndex = FindVertexIndex(collision.contacts[0].point);
            //Debug.Log($"Vertex Index : {vertIndex}");

            //Vector3 vertPos = GetVertexPoisition(vertIndex);
            //Debug.Log($"Vertex Pos : {vertPos}");

            //_footPrints.Add(vertPos);
        }

        //List<Vector3> _footPrints = new List<Vector3>();
        //private void OnDrawGizmos()
        //{
        //    Gizmos.color = Color.red;
        //    foreach (var foot in _footPrints)
        //    {
        //        Gizmos.DrawWireSphere(foot, 0.1f);
        //    }
        //}
    }
}