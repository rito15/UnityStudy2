using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-01-12 AM 1:47:46
// 작성자 : Rito

public class PersonalCamera : MonoBehaviour
{
    public Transform Rig { get; private set; }
    public Camera Cam { get; private set; }

    public virtual void Init()
    {
        Rig = transform.parent;
        Cam = GetComponent<Camera>();
    }
}