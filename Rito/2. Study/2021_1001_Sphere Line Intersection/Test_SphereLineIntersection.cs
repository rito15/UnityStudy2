using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 날짜 : 2021-10-01 PM 5:22:09
// 작성자 : Rito

/// <summary> 
/// 
/// </summary>
public class Test_SphereLineIntersection : MonoBehaviour
{
    public Transform rayBegin;
    public Transform rayEnd;
    public Transform sphereCenter;
    public float sphereRadius = 3f;

    [Space]
    public Mesh sphereMesh;

    private void OnDrawGizmos()
    {
        if (!rayBegin || !rayEnd || !sphereCenter || !sphereMesh) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(rayBegin.position, 0.2f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(rayEnd.position, 0.2f);

        Gizmos.color = Color.white * 0.5f;
        Gizmos.DrawMesh(sphereMesh, 0, sphereCenter.position, Quaternion.identity, Vector3.one * sphereRadius * 2);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(rayBegin.position, rayEnd.position);

        Vector3? interPoint = RaycastToSphere(rayBegin.position, rayEnd.position, sphereCenter.position, sphereRadius);
        if (interPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(interPoint.Value, 0.3f);
        }
    }

    private Vector3? RaycastToSphere(Vector3 origin, Vector3 end, Vector3 sphereCenter, float sphereRadius)
    {
        ref Vector3 A = ref origin;
        ref Vector3 B = ref end;
        ref Vector3 S = ref sphereCenter;
        Vector3 AS = S - A;

        ref float r = ref sphereRadius;
        float r2 = r * r;
        float as2 = AS.sqrMagnitude;

        // A가 구체 내부에 위치한 경우
        if (as2 < r2) return null;

        float ab = (B - A).magnitude;
        float as_ = Mathf.Sqrt(as2);

        // 레이가 구체 표면까지의 최단거리보다도 짧은 경우
        if (ab < as_ - r) return null;

        Vector3 nAB = (B - A).normalized;
        float ac = Vector3.Dot(AS, nAB);

        // 레이의 진행 방향이 구체의 위치와 반대인 경우
        if (ac < 0) return null;

        float ac2 = ac * ac;
        float sc2 = as2 - ac2;

        // 교차점이 없는 경우
        if (sc2 > r2) return null;

        float cd = Mathf.Sqrt(r2 - sc2);
        float ad = ac - cd;

        // 레이의 도착점이 구체 표면보다 레이 시작점에 가까울 경우
        if (ab < ad) return null;

        Vector3 D = A + nAB * ad;
        return D;
    }

    // 구체 여러 개 있을 경우, 가장 가까운 구체 찾기
    // RaycastToNearSphere
}