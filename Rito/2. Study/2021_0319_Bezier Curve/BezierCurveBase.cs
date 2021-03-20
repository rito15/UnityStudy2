using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-03-20 PM 5:31:19
// 작성자 : Rito

namespace Rito.BezierCurveTest
{
    public class BezierCurveBase : MonoBehaviour
    {
        [Range(0.01f, 1f)]
        public float gizmoRadius = 0.3f;

        [Range(0f, 1f)]
        public float progression = 0f;

        protected bool HasNull<T>(params T[] elements) where T : class
        {
            for (int i = 0; i < elements.Length; i++)
                if (elements[i] == null) return true;
            return false;
        }

        protected Vector3 Lerp(Transform a, Transform b, float t)
        {
            return Vector3.Lerp(a.position, b.position, t);
        }

        protected void DrawGizmoSphere(float radius, Transform point)
        {
            Gizmos.DrawWireSphere(point.position, radius);
        }

        protected void DrawGizmoSphere(float radius, in Vector3 point)
        {
            Gizmos.DrawWireSphere(point, radius);
        }

        protected void DrawGizmoSpheres(float radius, params Vector3[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                Gizmos.DrawWireSphere(points[i], radius);
            }
        }

        protected void DrawGizmoSpheres(float radius, params Transform[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                Gizmos.DrawWireSphere(points[i].position, radius);
            }
        }

        protected void DrawGizmoLine(Transform a, Transform b)
        {
            Gizmos.DrawLine(a.position, b.position);
        }

        protected void DrawGizmoLine(in Vector3 a, in Vector3 b)
        {
            Gizmos.DrawLine(a, b);
        }

        protected void DrawGizmoLines(params Vector3[] points)
        {
            int len = points.Length;
            if (len < 2) return;

            for (int i = 0; i < len - 1; i++)
            {
                Gizmos.DrawLine(points[i], points[i + 1]);
            }
        }

        protected void DrawGizmoLines(params Transform[] points)
        {
            int len = points.Length;
            if (len < 2) return;

            for (int i = 0; i < len - 1; i++)
            {
                Gizmos.DrawLine(points[i].position, points[i + 1].position);
            }
        }
    }
}