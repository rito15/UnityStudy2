using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-03-19 PM 4:11:50
// 작성자 : Rito

public class Test_QuadraticBezierCurve : MonoBehaviour
{
    [Range(0.01f, 1f)]
    public float gizmoRadius = 0.3f;

    [Range(0f, 1f)]
    public float progression = 0f;


    [Space]
    public Transform startPoint;
    public Transform endPoint;
    public Transform lerpPoint;

    private Vector3 lerpA;
    private Vector3 lerpB;
    private Vector3 lerpAB;

    private void OnValidate()
    {
        if(startPoint == null) return;
        if(endPoint == null) return;
        if(lerpPoint == null) return;

        lerpA = LerpPosition(startPoint, lerpPoint, progression);
        lerpB = LerpPosition(lerpPoint, endPoint, progression);
        lerpAB = Vector3.Lerp(lerpA, lerpB, progression);
    }

    private void OnDrawGizmos()
    {
        if (startPoint == null) return;
        if (endPoint == null) return;
        if (lerpPoint == null) return;

        // Draw Points
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(startPoint.position, gizmoRadius);
        Gizmos.DrawWireSphere(lerpA, gizmoRadius * 0.5f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(endPoint.position, gizmoRadius);
        Gizmos.DrawWireSphere(lerpB, gizmoRadius * 0.5f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(lerpPoint.position, gizmoRadius);

        // Draw Self
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(lerpAB, gizmoRadius);
    }

    private Vector3 LerpPosition(Transform trA, Transform trB, float t)
    {
        return Vector3.Lerp(trA.position, trB.position, t);
    }
}