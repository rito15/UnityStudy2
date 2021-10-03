using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 날짜 : 2021-10-03 PM 9:55:44
// 작성자 : Rito

/// <summary> 
/// 
/// </summary>
public class PlaneLineIntersection : MonoBehaviour
{
    public Transform rayOrigin;
    public Transform rayEnd;
    public Transform plane;

    public bool intersected;

    private void OnDrawGizmos()
    {
        if (!rayOrigin || !rayEnd || !plane) return;

        Vector3 ro = rayOrigin.position;
        Vector3 re = rayEnd.position;
        Vector3 pp = plane.position;
        Vector3 pn = plane.up;

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(ro, 0.3f);
        Gizmos.DrawLine(ro, re);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(re, 0.3f);

        Vector3? intersection = RaycastToPlane(ro, re, pp, pn);
        intersected = (intersection != null);
        if (intersected)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(intersection.Value, 0.3f);
        }
    }

    private Vector3? RaycastToPlane(Vector3 origin, Vector3 end, Vector3 planePoint, Vector3 planeNormal)
    {
        ref Vector3 A = ref origin;
        ref Vector3 B = ref end;
        ref Vector3 P = ref planePoint;
        ref Vector3 N = ref planeNormal;
        Vector3 AB = (B - A);
        Vector3 nAB = AB.normalized;

        float d = Vector3.Dot(N, P - A) / Vector3.Dot(N, nAB);

        // 레이 방향이 평면을 향하지 않는 경우
        if (d < 0) return null;

        Vector3 C = A + nAB * d;

        float sqrAB = AB.sqrMagnitude;
        float sqrAC = (C - A).sqrMagnitude;

        // 레이가 짧아서 평면에 도달하지 못한 경우
        if (sqrAB < sqrAC) return null;

        return C;
    }
}