using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 날짜 : 2021-04-21 PM 10:13:16
// 작성자 : Rito

namespace Rito.BresenhamAlgorithm
{
    public class LineDrawer : MonoBehaviour
    {
        private Image _lineImage;
        private RectTransform _rt;

        public RectTransform _pointA;
        public RectTransform _pointB;

        private void Start()
        {
            TryGetComponent(out _lineImage);
            _rt = _lineImage.rectTransform;
        }

        private void Update()
        {
            Vector2 posA = _pointA.anchoredPosition;
            Vector2 posB = _pointB.anchoredPosition;

            float len = Vector2.Distance(posA, posB);
            float angle = Mathf.Atan2(posB.y - posA.y, posB.x - posA.x) * Mathf.Rad2Deg;

            _rt.position = _pointA.position;
            _rt.sizeDelta = new Vector2(len, _rt.sizeDelta.y);

            var rot = _rt.localEulerAngles;
            _rt.localEulerAngles = new Vector3(rot.x, rot.y, angle);
        }
    }
}