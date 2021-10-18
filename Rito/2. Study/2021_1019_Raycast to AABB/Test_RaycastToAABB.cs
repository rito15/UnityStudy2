using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 날짜 : 2021-10-19 AM 2:46:46
// 작성자 : Rito

/// <summary> 
/// 
/// </summary>
public class Test_RaycastToAABB : MonoBehaviour
{
    public Transform rayOrigin;
    public Transform rayEnd;
    public Transform cube;

    private void OnDrawGizmos()
    {
        if (!rayOrigin || !rayEnd || !cube) return;

        Bounds bounds = new Bounds(cube.position, cube.lossyScale);
        MinMax minMax = MinMax.FromBounds(bounds);

        Vector3 A = rayOrigin.position;
        Vector3 B = rayEnd.position;
        Vector3? contact = RaycastToAABB(A, B, minMax);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(A, 0.3f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(B, 0.3f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(A, B);

        if (contact.HasValue)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(contact.Value, 0.3f);
        }
    }

    private struct MinMax
    {
        public Vector3 min;
        public Vector3 max;
        public static MinMax FromBounds(in Bounds bounds)
        {
            MinMax mm = default;
            mm.min = bounds.min;
            mm.max = bounds.max;
            return mm;
        }
    }

    /// <summary> XY 평면에 정렬된 평면을 향해 레이캐스트 </summary>
    private Vector3 RaycastToPlaneXY(in Vector3 A, in Vector3 B, float planeZ)
    {
        float ratio = (B.z - planeZ) / (B.z - A.z);
        Vector3 C;
        C.x = (A.x - B.x) * ratio + (B.x);
        C.y = (A.y - B.y) * ratio + (B.y);
        C.z = planeZ;
        return C;
    }
    /// <summary> XZ 평면에 정렬된 평면을 향해 레이캐스트 </summary>
    private Vector3 RaycastToPlaneXZ(in Vector3 A, in Vector3 B, float planeY)
    {
        float ratio = (B.y - planeY) / (B.y - A.y);
        Vector3 C;
        C.x = (A.x - B.x) * ratio + (B.x);
        C.z = (A.z - B.z) * ratio + (B.z);
        C.y = planeY;
        return C;
    }
    /// <summary> YZ 평면에 정렬된 평면을 향해 레이캐스트 </summary>
    private Vector3 RaycastToPlaneYZ(in Vector3 A, in Vector3 B, float planeX)
    {
        float ratio = (B.x - planeX) / (B.x - A.x);
        Vector3 C;
        C.y = (A.y - B.y) * ratio + (B.y);
        C.z = (A.z - B.z) * ratio + (B.z);
        C.x = planeX;
        return C;
    }

    /// <summary> 벡터의 각 요소마다 부호값 계산 </summary>
    private Vector3 Sign(in Vector3 vec)
    {
        return new Vector3(
            vec.x >= 0f ? 1f : -1f,
            vec.y >= 0f ? 1f : -1f,
            vec.z >= 0f ? 1f : -1f
        );
    }

    /// <summary> 값이 닫힌 범위 내에 있는지 검사 </summary>
    private bool InRange(float value, float min, float max)
    {
        return min <= value && value <= max;
    }

    private Vector3? RaycastToAABB(Vector3 origin, Vector3 end, in MinMax bounds)
    {
        ref Vector3 A = ref origin;
        ref Vector3 B = ref end;
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        Vector3 AB = B - A;
        Vector3 signAB = Sign(AB);
        Vector3 contact;

        // [1] YZ 평면 검사
        if (signAB.x > 0) contact = RaycastToPlaneYZ(A, B, min.x);
        else              contact = RaycastToPlaneYZ(A, B, max.x);

        if (InRange(contact.y, min.y, max.y) && InRange(contact.z, min.z, max.z))
            return contact;
        
        // [2] XZ 평면 검사
        if (signAB.y > 0) contact = RaycastToPlaneXZ(A, B, min.y);
        else              contact = RaycastToPlaneXZ(A, B, max.y);

        if (InRange(contact.x, min.x, max.x) && InRange(contact.z, min.z, max.z))
            return contact;

        // [3] XY 평면 검사
        if (signAB.z > 0) contact = RaycastToPlaneXY(A, B, min.z);
        else              contact = RaycastToPlaneXY(A, B, max.z);

        if (InRange(contact.x, min.x, max.x) && InRange(contact.y, min.y, max.y))
            return contact;

        // [4] No Contact Point
        return null;
    }
}