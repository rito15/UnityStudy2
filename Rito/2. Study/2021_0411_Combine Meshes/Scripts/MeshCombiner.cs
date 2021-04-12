using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-04-11 PM 5:49:02
// 작성자 : Rito

namespace Rito.CombineMesh
{
    /*
        [메시 병합]
        - static으로 설정하는 것보다 성능 향상이 월등함
        - 대신 통합된 메시마다 각각 새로운 메시를 메모리에 할당하므로, 메모리 많이 잡아먹는 단점
        
        - TODO : 런타임에 추가 병합 / 병합된 것 해제
        - TODO : 해제를 위해서는 각각 메시마다 Bound(Vector3 Min/Max)로 관리되고,
                 특정 좌표를 입력했을 때 빠르게 해당 메시의 시작 인덱스를 찾아올 수 있어야 함
    */

    public class MeshCombiner : MonoBehaviour
    {
        /// <summary> 통합 메시 관리 클래스 </summary>
        private class CombinedMeshPool
        {
            public int currentIndex = -1;

            public List<CombinedMeshData> list = new List<CombinedMeshData>();

            /// <summary> 새로운 메시 통합 </summary>
            public void Add(Transform target)
            {
                target.TryGetComponent(out MeshFilter mf);

                // 메시 필터가 없는 경우 제외
                //if(mf == null) return;

                target.TryGetComponent(out MeshRenderer mr);

                // 초기, 수용 한계 넘은 경우 : 새로운 통합 메시 생성
                if (currentIndex == -1 || !list[currentIndex].IsAvailable)
                {
                    currentIndex++;
                    list.Add(new CombinedMeshData(mf, mr, currentIndex));
                }

                list[currentIndex].AddMesh(target);
            }

            /// <summary> 통합 메시 모두 적용 </summary>
            public void ApplyAll()
            {
                foreach (var combinedMeshData in list)
                {
                    combinedMeshData.Apply();
                    combinedMeshData.DestroyOriginals();
                }
            }
        }

        /// <summary> 통합 메시 데이터 </summary>
        private class CombinedMeshData
        {
            private Mesh sharedMesh;          // 공유되는 원본 메시
            private Mesh combinedMesh;        // 통합 메시

            private const int MaxVertCount = 65535; // 최대 버텍스 개수 제한
            private readonly int VertexCount; // 한 메시의 버텍스 개수

            private int currentVertCount;     // 현재 통합 메시의 버텍스 개수 총합
            private int combinedMeshCount;    // 현재까지 통합된 메시 개수

            /// <summary> 메시를 더 추가할 수 있는지 여부 </summary>
            public bool IsAvailable
                => (currentVertCount + VertexCount) < MaxVertCount;

            private GameObject combinedGameObject;
            private MeshFilter combinedFilter;
            private MeshRenderer combinedRenderer;

            private List<Vector3> vertList = new List<Vector3>();
            private List<int>     triList  = new List<int>();
            private List<Vector2> uvList   = new List<Vector2>();
            private List<Vector3> normalList  = new List<Vector3>();
            private List<Vector4> tangentList = new List<Vector4>();

            // 모든 작업이 끝나고 파괴될 대상 게임오브젝트들(원본들)
            private List<GameObject> originGoList = new List<GameObject>();

            public CombinedMeshData(MeshFilter targetFilter, MeshRenderer targetRenderer, int index)
            {
                sharedMesh = targetFilter.sharedMesh;

                // Components
                combinedGameObject = new GameObject($"Combined {sharedMesh.name} [index : {index}");
                combinedFilter   = combinedGameObject.AddComponent<MeshFilter>();
                combinedRenderer = combinedGameObject.AddComponent<MeshRenderer>();

                // Combined Mesh
                combinedMesh = new Mesh();
                combinedFilter.mesh = combinedMesh;

                VertexCount = sharedMesh.vertexCount;
                combinedRenderer.sharedMaterial = targetRenderer.sharedMaterial;

                currentVertCount = 0;
                combinedMeshCount = 0;
            }

            /// <summary> 통합 메시에 새로운 메시 추가 </summary>
            public void AddMesh(Transform target)
            {
                target.SetParent(null);

                Matrix4x4 mat = target.localToWorldMatrix;

                // 버텍스 공간변환하여 리스트에 추가
                foreach (var vert in sharedMesh.vertices)
                {
                    Vector3 vert1 = mat.MultiplyPoint(vert);
                    vertList.Add(vert1);
                }

                // 삼각형 추가
                foreach (var tri in sharedMesh.triangles)
                {
                    triList.Add(tri + currentVertCount);
                }

                // UV 추가
                uvList.AddRange(sharedMesh.uv);

                // 노말, 탄젠트 공간변환하여 추가
                foreach (var normal in sharedMesh.normals)
                {
                    Vector3 normal1 = mat.MultiplyVector(normal);
                    normalList.Add(normal1);
                }
                foreach (var tangent in sharedMesh.tangents)
                {
                    Vector3 tangent1 = mat.MultiplyVector(tangent);
                    tangentList.Add(tangent1);
                }

                combinedMeshCount++;
                currentVertCount += VertexCount;
                originGoList.Add(target.gameObject);
            }

            /// <summary> 메시 필터에 통합 메시 적용 </summary>
            public void Apply()
            {
                combinedMesh.vertices = vertList.ToArray();
                combinedMesh.triangles = triList.ToArray();
                combinedMesh.uv = uvList.ToArray();
                combinedMesh.normals = normalList.ToArray();
                combinedMesh.tangents = tangentList.ToArray();
                combinedMesh.RecalculateBounds();

                combinedGameObject.name += $", Meshes : {combinedMeshCount}]";
                combinedMesh.name = $"{sharedMesh.name} (Combined : {combinedMeshCount})";
            }

            /// <summary> 원본 게임오브젝트들 모두 파괴 </summary>
            public void DestroyOriginals()
            {
                for (int i = 0; i < originGoList.Count; i++)
                {
                    Destroy(originGoList[i]);
                }
                originGoList = null;
            }
        }

        private Dictionary<int, CombinedMeshPool> _meshPoolDict = new Dictionary<int, CombinedMeshPool>();

        private void Start()
        {
            var targets = FindObjectsOfType<MeshCombinationTarget>();
            foreach (var target in targets)
            {
                MeshFilter mf = target.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    int instanceID = mf.sharedMesh.GetInstanceID();

                    _meshPoolDict.TryGetValue(instanceID, out var pool);
                    if (pool == null)
                    {
                        pool = new CombinedMeshPool();
                        _meshPoolDict.Add(instanceID, pool);
                    }

                    pool.Add(target.transform);
                }
            }

            foreach (var pool in _meshPoolDict.Values)
            {
                pool.ApplyAll();
            }
        }
    }
}