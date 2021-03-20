using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-03-20 PM 5:19:26
// 작성자 : Rito

namespace Rito.BezierCurveTest
{
    // 3차 베지어 커브
    public class CubicBezierCurve : BezierCurveBase
    {
        [Space]
        public Transform pointA;
        public Transform pointB;
        public Transform pointC;
        public Transform pointD;

        private Vector3 lerpAB;
        private Vector3 lerpBC;
        private Vector3 lerpCD;
        private Vector3 lerpABC;
        private Vector3 lerpBCD;
        private Vector3 lerpABCD;

        private void OnValidate()
        {
            if (pointA == null || pointB == null || pointC == null || pointD == null) return;

            lerpAB = Lerp(pointA, pointB, progression);
            lerpBC = Lerp(pointB, pointC, progression);
            lerpCD = Lerp(pointC, pointD, progression);
            lerpABC = Vector3.Lerp(lerpAB, lerpBC, progression);
            lerpBCD = Vector3.Lerp(lerpBC, lerpCD, progression);
            lerpABCD = Vector3.Lerp(lerpABC, lerpBCD, progression);
        }

        private void OnDrawGizmos()
        {
            if (pointA == null || pointB == null || pointC == null || pointD == null) return;

            float radius = gizmoRadius;

            Gizmos.color = Color.red;
            DrawGizmoSpheres(radius, pointA, pointB, pointC, pointD);
            DrawGizmoLines(pointA, pointB, pointC, pointD);

            Gizmos.color = Color.blue;
            radius *= 0.8f;
            DrawGizmoSpheres(radius, lerpAB, lerpBC, lerpCD);
            DrawGizmoLines(lerpAB, lerpBC, lerpCD);

            Gizmos.color = Color.magenta;
            radius *= 0.8f;
            DrawGizmoSpheres(radius, lerpABC, lerpBCD);
            DrawGizmoLine(lerpABC, lerpBCD);

            Gizmos.color = Color.yellow;
            radius *= 0.8f;
            DrawGizmoSphere(radius, lerpABCD);

            if (Application.isPlaying)
            {
                DrawCurve();
            }
        }

        private void Awake()
        {
            CalculateCurvePoints(200);
        }


        private Vector3[] curvePoints;
        private void CalculateCurvePoints(int count)
        {
            curvePoints = new Vector3[count + 1];
            float unit = 1.0f / count;

            int i = 0; float t = 0f;
            for (; i < count + 1; i++, t += unit)
            {
                Vector3 pA = pointA.position, pB = pointB.position,
                    pC = pointC.position, pD = pointD.position;
                float t2 = t * t;
                float t3 = t * t2;
                float u = (1 - t);
                float u2 = u * u;
                float u3 = u * u2;

                curvePoints[i] =
                    pA * u3 +
                    pB * t * u2 * 3 +
                    pC * t2 * u * 3 +
                    pD * t3
                    ;
            }
        }

        private void DrawCurve()
        {
            if (curvePoints == null || curvePoints.Length == 0) return;

            float fLen = (curvePoints.Length - 1) * progression;
            for (int i = 0; i < fLen; i++)
            {
                Gizmos.DrawLine(curvePoints[i], curvePoints[i + 1]);
            }
        }
    }
}