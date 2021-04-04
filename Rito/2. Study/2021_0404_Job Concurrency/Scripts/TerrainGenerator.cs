using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Jobs;
using System.Threading.Tasks;
using System.Threading;

// 날짜 : 2021-04-04 PM 7:24:50
// 작성자 : Rito

namespace Rito.JobTest
{
    public class TerrainGenerator : MonoBehaviour
    {
        private enum TestCase
        {
            Basic, JobSync, JobAsync
        }
        [SerializeField] private TestCase _testCase;

        [Space(16)]
        [SerializeField] private int _resolution = 9; // XZ 각각 버텍스 개수
        [SerializeField] private float _width = 10;   // 한 청크의 XZ 크기 
        [SerializeField] private int _chunkCount = 4; // XZ 각각 청크 개수
        [SerializeField] private float _maxHeight = 4f; // 터레인의 최대 높이
        [SerializeField] private float _noiseScale = 10f; // 노이즈 스케일

        [SerializeField] private Material _material;

        private class MeshData
        {
            public List<Vector3> vertList;
            public List<int> trisList;

            public MeshData()
            {
                vertList = new List<Vector3>();
                trisList = new List<int>();
            }
        }

        private async void TaskTest()
        {
            await Task.Run(() =>
            {
                MainThreadDispatcher.Instance.Enqueue(() => transform.Translate(1f, 0f, 0f));

                //transform.Translate(1f, 0f, 0f);
                Debug.Log("Task");
                Debug.Log(Thread.CurrentThread.ManagedThreadId);
            });
        }

        private void Start1()
        {
            //TaskTest();
            //return;

            switch (_testCase)
            {
                case TestCase.Basic: TestBasic();
                    break;
                case TestCase.JobSync: TestJobSync();
                    break;
                case TestCase.JobAsync: StartCoroutine(TestJobAsyncRoutine());
                    break;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                switch (_testCase)
                {
                    case TestCase.Basic:
                        TestBasic();
                        break;
                    case TestCase.JobSync:
                        TestJobSync();
                        break;
                    case TestCase.JobAsync:
                        StartCoroutine(TestJobAsyncRoutine());
                        break;
                }
            }
        }

        /***********************************************************************
        *                               Test Basic
        ***********************************************************************/
        #region .
        private void TestBasic()
        {
            Vector3 curChunkPos = Vector3.zero;

            for (int z = 0; z < _chunkCount; z++)
            {
                curChunkPos.x = 0f;

                for (int x = 0; x < _chunkCount; x++)
                {
                    MeshData meshData = new MeshData();
                    CalculateMesh(meshData, curChunkPos);
                    GenerateMesh(meshData);

                    curChunkPos.x += _width;
                }

                curChunkPos.z += _width;
            }
        }

        private void CalculateMesh(MeshData meshData, in Vector3 startPos)
        {
            float xzUnit = _width / _resolution;

            Vector3 curVertPos = startPos;

            // 1. 버텍스 생성
            for (int z = 0; z < _resolution; z++)
            {
                curVertPos.x = startPos.x;

                for (int x = 0; x < _resolution; x++)
                {
                    curVertPos.y = GetPerlinHeight(curVertPos.x, curVertPos.z, _noiseScale) * _maxHeight;
                    meshData.vertList.Add(curVertPos);

                    curVertPos.x += xzUnit;
                }

                curVertPos.z += xzUnit;
            }

            // 2. 폴리곤 조립
            for (int z = 0; z < _resolution - 1; z++)
            {
                for (int x = 0; x < _resolution - 1; x++)
                {
                    int LB = x + (z * _resolution); // LB Index

                    meshData.trisList.Add(LB);
                    meshData.trisList.Add(LB + _resolution);
                    meshData.trisList.Add(LB + 1);

                    meshData.trisList.Add(LB + 1);
                    meshData.trisList.Add(LB + _resolution);
                    meshData.trisList.Add(LB + _resolution + 1);
                }
            }
        }

        private float GetPerlinHeight(float x, float y, float scale)
        {
            return Mathf.PerlinNoise(x / scale + 0.1f, y / scale + 0.1f);
        }

        private void GenerateMesh(MeshData meshData)
        {
            GameObject go = new GameObject("Terrain");
            var mFilter = go.AddComponent<MeshFilter>();
            var mRender = go.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            mesh.vertices = meshData.vertList.ToArray();
            mesh.triangles = meshData.trisList.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            mFilter.mesh = mesh;
            mRender.sharedMaterial = _material;
        }

        #endregion
        /***********************************************************************
        *                               Test Job Sync
        ***********************************************************************/
        #region .
        private void TestJobSync()
        {
            Vector3 curChunkPos = Vector3.zero;

            for (int z = 0; z < _chunkCount; z++)
            {
                curChunkPos.x = 0f;

                for (int x = 0; x < _chunkCount; x++)
                {
                    TerrainJob job = new TerrainJob(
                        _resolution, _width, _maxHeight, _noiseScale, curChunkPos
                    );

                    var handle = job.Schedule();
                    handle.Complete(); // 메인 스레드에서 대기

                    var result = job.GetResults();
                    GenerateMesh(result.verts, result.tris);

                    curChunkPos.x += _width;
                }

                curChunkPos.z += _width;
            }
        }

        private void GenerateMesh(Vector3[] verts, int[] tris)
        {
            GameObject go = new GameObject("Terrain");
            var mFilter = go.AddComponent<MeshFilter>();
            var mRender = go.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            mFilter.mesh = mesh;
            mRender.sharedMaterial = _material;
        }

        #endregion
        /***********************************************************************
        *                               Test Job Async
        ***********************************************************************/
        #region .
        private IEnumerator TestJobAsyncRoutine()
        {
            Vector3 curChunkPos = Vector3.zero;

            for (int z = 0; z < _chunkCount; z++)
            {
                curChunkPos.x = 0f;

                for (int x = 0; x < _chunkCount; x++)
                {
                    TerrainJob job = new TerrainJob(
                        _resolution, _width, _maxHeight, _noiseScale, curChunkPos
                    );

                    var handle = job.Schedule();

                    // 잡이 완료되지 않았다면 프레임 넘기기
                    while(!handle.IsCompleted)
                        yield return null;

                    handle.Complete();

                    var result = job.GetResults();
                    GenerateMesh(result.verts, result.tris);

                    curChunkPos.x += _width;
                }

                curChunkPos.z += _width;
            }
        }

        #endregion
    }
}