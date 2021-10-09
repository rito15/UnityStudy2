using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 날짜 : 2021-10-10 AM 3:09:13
// 작성자 : Rito

/// <summary> 
/// 
/// </summary>
public class Test_SphereCast : MonoBehaviour
{
    public Mesh sphereMesh;

    [Space]
    public Transform castOrigin;
    public Transform castEnd;
    public float castRadius;

    [Space]
    public Transform targetSphere;
    public float targetRadius;

    private void OnDrawGizmos()
    {
        if (!castOrigin || !castEnd || !targetSphere || !sphereMesh) return;

        Vector3 A = castOrigin.position;
        Vector3 B = castEnd.position;
        Vector3 S = targetSphere.position;
        float r1 = castRadius;
        float r2 = targetRadius;

        Gizmos.color = Color.red * 0.8f;
        Gizmos.DrawMesh(sphereMesh, A, Quaternion.identity, Vector3.one * 2f * r1);
        Gizmos.DrawMesh(sphereMesh, B, Quaternion.identity, Vector3.one * 0.8f);

        Gizmos.color = Color.blue * 0.8f;
        Gizmos.DrawMesh(sphereMesh, S, Quaternion.identity, Vector3.one * 2f * r2);

        Vector3? contact = SphereCastToSphere(A, B, S, r1, r2);
        if (contact != null)
        {
            Gizmos.color = Color.yellow * 0.8f;
            Gizmos.DrawMesh(sphereMesh, contact.Value, Quaternion.identity, Vector3.one * 2f * r1);
        }
    }

    private Vector3? SphereCastToSphere(Vector3 origin, Vector3 end, Vector3 targetSphere, float castRadius, float targetRadius)
    {
        ref Vector3 A = ref origin;
        ref Vector3 B = ref end;
        ref Vector3 S = ref targetSphere;
        ref float r1 = ref castRadius;
        ref float r2 = ref targetRadius;

        Vector3 AB  = (B - A);
        Vector3 nAB = AB.normalized;
        Vector3 AS  = (S - A);

        float ab  = AB.magnitude;
        float as2 = AS.sqrMagnitude;
        float as_ = Mathf.Sqrt(as2);

        // 캐스트(A->B) 거리가 너무 가까운 경우
        if (ab + r1 < as_ - r2) return null;

        float ad  = Vector3.Dot(AS, nAB);

        // 캐스트 방향이 반대인 경우
        if (ad < 0) return null;

        float ad2 = ad * ad;
        float ds2 = as2 - ad2;
        float ds  = Mathf.Sqrt(ds2);
        float cs  = r1 + r2;

        // S에서 AB에 내린 수선의 길이가 두 구체의 반지름 합보다 긴 경우
        if (ds > cs) return null;

        float cs2 = cs * cs;
        float cd  = Mathf.Sqrt(cs2 - ds2);
        float ac  = ad - cd;

        Vector3 C = A + nAB * ac;
        //Vector3 E = C + (S - C) * r1 / cs; // 충돌 지점 좌표

        return C;
    }

    // 충돌 여부를 미리 알고 있는 경우 사용하는 간소화 메소드
    private Vector3 SphereCastToSphere_Simple(Vector3 origin, Vector3 end, Vector3 targetSphere, float castRadius, float targetRadius)
    {
        ref Vector3 A = ref origin;
        ref Vector3 B = ref end;
        ref Vector3 S = ref targetSphere;
        ref float r1 = ref castRadius;
        ref float r2 = ref targetRadius;

        Vector3 nAB = (B - A).normalized;
        Vector3 AS  = (S - A);
        float as2 = AS.sqrMagnitude;
        float ad  = Vector3.Dot(AS, nAB);
        float ad2 = ad * ad;
        float ds2 = as2 - ad2;
        float cs  = r1 + r2;
        float cs2 = cs * cs;
        float cd  = Mathf.Sqrt(cs2 - ds2);
        float ac  = ad - cd;

        Vector3 C = A + nAB * ac;            // 충돌 시 구체 중심 좌표
        //Vector3 E = C + (S - C) * r1 / cs; // 충돌 지점 좌표
        return C;
    }
}