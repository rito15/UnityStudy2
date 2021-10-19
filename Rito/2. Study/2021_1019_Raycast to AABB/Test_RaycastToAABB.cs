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
        float ratio = (A.z - planeZ) / (A.z - B.z);
        Vector3 C;
        C.x = (B.x - A.x) * ratio + (A.x);
        C.y = (B.y - A.y) * ratio + (A.y);
        C.z = planeZ;
        return C;
    }
    /// <summary> XZ 평면에 정렬된 평면을 향해 레이캐스트 </summary>
    private Vector3 RaycastToPlaneXZ(in Vector3 A, in Vector3 B, float planeY)
    {
        float ratio = (A.y - planeY) / (A.y - B.y);
        Vector3 C;
        C.x = (B.x - A.x) * ratio + (A.x);
        C.z = (B.z - A.z) * ratio + (A.z);
        C.y = planeY;
        return C;
    }
    /// <summary> YZ 평면에 정렬된 평면을 향해 레이캐스트 </summary>
    private Vector3 RaycastToPlaneYZ(in Vector3 A, in Vector3 B, float planeX)
    {
        float ratio = (A.x - planeX) / (A.x - B.x);
        Vector3 C;
        C.y = (B.y - A.y) * ratio + (A.y);
        C.z = (B.z - A.z) * ratio + (A.z);
        C.x = planeX;
        return C;
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
        Vector3 contact;

        // [1] YZ 평면 검사
        if (AB.x > 0) contact = RaycastToPlaneYZ(A, B, min.x);
        else          contact = RaycastToPlaneYZ(A, B, max.x);

        if (InRange(contact.y, min.y, max.y) && InRange(contact.z, min.z, max.z))
            goto VALIDATE_DISTANCE;

        // [2] XZ 평면 검사
        if (AB.y > 0) contact = RaycastToPlaneXZ(A, B, min.y);
        else          contact = RaycastToPlaneXZ(A, B, max.y);

        if (InRange(contact.x, min.x, max.x) && InRange(contact.z, min.z, max.z))
            goto VALIDATE_DISTANCE;

        // [3] XY 평면 검사
        if (AB.z > 0) contact = RaycastToPlaneXY(A, B, min.z);
        else          contact = RaycastToPlaneXY(A, B, max.z);

        if (InRange(contact.x, min.x, max.x) && InRange(contact.y, min.y, max.y))
            goto VALIDATE_DISTANCE;

        // [4] No Contact Point
        return null;

        // 길이 검사 : 교점이 레이보다 더 긴 경우 제외
    VALIDATE_DISTANCE:
        float ab2 = AB.sqrMagnitude;
        float len = (contact - A).sqrMagnitude;

        if (ab2 < len)
            return null;
        else
            return contact;
    }
}