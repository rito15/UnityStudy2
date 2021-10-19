using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-03-19 PM 4:11:50
// 작성자 : Rito


namespace Rito.BezierCurveTest
{
    // 2차 베지어 커브
    public class QuadraticBezierCurve : BezierCurveBase
    {
        [Space]
        public Transform pointA;
        public Transform pointB;
        public Transform pointC;

        private Vector3 lerpAB;
        private Vector3 lerpBC;
        private Vector3 lerpABC;

        private void OnValidate()
        {
            if (pointA == null || pointB == null || pointC == null) return;

            lerpAB = Lerp(pointA, pointB, progression);
            lerpBC = Lerp(pointB, pointC, progression);
            lerpABC = Vector3.Lerp(lerpAB, lerpBC, progression);
        }

        private void OnDrawGizmos()
        {
            if (pointA == null || pointB == null || pointC == null) return;

            float radius = gizmoRadius;

            Gizmos.color = Color.red;
            DrawGizmoSpheres(radius, pointA, pointB, pointC);
            DrawGizmoLines(pointA, pointB, pointC);

            Gizmos.color = Color.blue;
            radius *= 0.8f;
            DrawGizmoSpheres(radius, lerpAB, lerpBC);
            DrawGizmoLine(lerpAB, lerpBC);

            Gizmos.color = Color.yellow;
            radius *= 0.8f;
            DrawGizmoSphere(radius, lerpABC);

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

        /// <summary> 베지어 커브 내의 지점들 미리 계산 </summary>
        private void CalculateCurvePoints(int count)
        {
            Vector3 pA = pointA.position;
            Vector3 pB = pointB.position;
            Vector3 pC = pointC.position;

            curvePoints = new Vector3[count + 1];
            float unit = 1.0f / count;

            int i = 0; float t = 0f;
            for (; i < count + 1; i++, t += unit)
            {
                float u = (1 - t);
                float t2 = t * t;
                float u2 = u * u;

                curvePoints[i] = 
                    pA *       u2      + 
                    pB * (t  * u  * 2) + 
                    pC * t2
                ;
            }
        }
        private void DrawCurve()
        {
            if(curvePoints == null || curvePoints.Length == 0) return;

            float fLen = (curvePoints.Length - 1) * progression;
            for (int i = 0; i < fLen; i++)
            {
                Gizmos.DrawLine(curvePoints[i], curvePoints[i + 1]);
            }
        }
    
    }
}