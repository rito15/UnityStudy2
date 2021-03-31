using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 날짜 : 2021-03-31 PM 3:09:09
// 작성자 : Rito

public class Test_InputModule : StandaloneInputModule
{
    PointerEventData ped;

    protected override void Start()
    {
        Debug.Log("Start");
        ped = new PointerEventData(EventSystem.current);

        ped.position = Vector2.one * 100f;

        this.ProcessMove(ped);
        InvokeMouseClick();
    }

    public void InvokeMouseClick()
    {
        Debug.Log("InvokeMouseClick");

        var mouseData = GetMousePointerEventData(0);
        var leftButtonData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;

        leftButtonData.buttonData.position = Vector2.one * 100f;
        leftButtonData.buttonState = PointerEventData.FramePressState.PressedAndReleased;

        ProcessMousePress(leftButtonData);
    }

    public void InvokeMousePointerMove(in Vector2 pos)
    {
        // Todo
        
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
            InvokeMouseClick();

        if(Input.GetMouseButtonDown(0))
            Debug.Log("Left Mouse Down"); // 이건 안됨


    }
}