using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 날짜 : 2021-10-26 PM 6:28:56
// 작성자 : Rito

/// <summary> 
/// 
/// </summary>
public class Test_SphereAABBIntersection : MonoBehaviour
{
    public Transform cube;
    public Transform sphere;
    public Mesh sphereMesh;

    private void OnDrawGizmos()
    {
        if (!cube || !sphere) return;

        Vector3 S = sphere.position;
        float r   = sphere.lossyScale.x * 0.5f;

        Bounds b  = new Bounds(cube.position, cube.lossyScale);
        AABB aabb = AABB.FromBounds(b);

        // Sphere - AABB 최단 지점
        Vector3 C = ClosestPointToAABB(S, aabb);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(S, C);

        // Sphere - AABB 교차 검사
        if (SphereAABBIntersection(S, r, aabb))
        {
            Gizmos.DrawSphere(C, 0.1f);

            if (sphereMesh)
            {
                Gizmos.color = Color.yellow * 0.6f;
                Gizmos.DrawMesh(sphereMesh, S, Quaternion.identity, sphere.lossyScale * 1.01f);
            }
        }
    }

    private struct AABB
    {
        public Vector3 min;
        public Vector3 max;

        public static AABB FromBounds(in Bounds b)
        {
            return new AABB { min = b.min, max = b.max };
        }
    }

    /// <summary> 구체와 AABB의 교차 여부 확인 </summary>
    private bool SphereAABBIntersection(in Vector3 S, in float r, in AABB aabb)
    {
        Vector3 C = ClosestPointToAABB(S, aabb);
        return (C - S).sqrMagnitude <= r * r;
    }

    /// <summary> 한 점으로부터 AABB 위의 최단 지점 계산 </summary>
    private Vector3 ClosestPointToAABB(Vector3 P, in AABB aabb)
    {
        if      (P.x < aabb.min.x) P.x = aabb.min.x;
        else if (P.x > aabb.max.x) P.x = aabb.max.x;
        if      (P.y < aabb.min.y) P.y = aabb.min.y;
        else if (P.y > aabb.max.y) P.y = aabb.max.y;
        if      (P.z < aabb.min.z) P.z = aabb.min.z;
        else if (P.z > aabb.max.z) P.z = aabb.max.z;
        return P;
    }
}