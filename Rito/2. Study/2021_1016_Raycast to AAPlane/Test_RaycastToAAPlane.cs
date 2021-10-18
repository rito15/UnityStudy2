using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 날짜 : 2021-10-16 AM 3:30:58
// 작성자 : Rito

/// <summary> 
/// 
/// </summary>
public class Test_RaycastToAAPlane : MonoBehaviour
{
    public Transform rayOrigin;
    public Transform rayEnd;
    public Transform xzPlane;

    private void OnDrawGizmos()
    {
        if (!rayOrigin || !rayEnd || !xzPlane) return;
        Vector3 A = rayOrigin.position;
        Vector3 B = rayEnd.position;
        float planeY = xzPlane.position.y;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(A, 0.3f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(B, 0.3f);

        Gizmos.color = (Color.red + Color.blue) * 0.5f;
        Gizmos.DrawLine(A, B);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(RaycastToPlaneXZ(A, B, planeY), 0.3f);
    }

    private Vector3 RaycastToPlaneXZ(Vector3 rayOrigin, Vector3 rayEnd, float planeY)
    {
        ref Vector3 A = ref rayOrigin;
        ref Vector3 B = ref rayEnd;
        float ratio = (B.y - planeY) / (B.y - A.y);

        Vector3 C;
        C.x = (A.x - B.x) * ratio + (B.x);
        C.z = (A.z - B.z) * ratio + (B.z);
        C.y = planeY;
        return C;
    }
}