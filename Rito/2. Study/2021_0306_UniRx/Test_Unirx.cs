using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

using UniRx;
using UniRx.Triggers;

using Rito.UnityLibrary.Tester;
using Debug = Rito.UnityLibrary.Debug;


// 날짜 : 2021-03-06 PM 8:44:40
// 작성자 : Rito

public class Test_Unirx : MonoBehaviour
{
    public Button _targetButton;
    public Button _targetButton2;

    [Range(0, 1)]
    public float _value;

    private void Start()
    {
        //EventAsStream();
        //DragAndDrop();
        //LifeSpan();
        CheckDoubleClickImmediatley();

        //CheckDoubleKeyPress(KeyCode.A, () => transform.Translate(Vector3.left * Time.deltaTime * 5f));
        //CheckDoubleKeyPress(KeyCode.D, () => transform.Translate(Vector3.right * Time.deltaTime * 5f));
        CheckDoubleKeyPressImmediatley(KeyCode.W, () => Debug.Log("WW"));

        //ObserveValueChanged();
    }

    // T o D o : 그냥 변수의 값 변화 인식

    private void Update()
    {



        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(_targetButton == null, ReferenceEquals(_targetButton, null));
            if(_targetButton == null)
                _targetButton = null;
        }
    }

    private void CheckDoubleClick()
    {
        // 좌클릭 입력을 감지하는 스트림 생성
        var dbClickStream = 
            Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButtonDown(0));

        // 스트림의 동작 정의, 종료 가능한 객체 반환
        var dbClickStreamDisposable =
            dbClickStream
                .Buffer(dbClickStream.Throttle(TimeSpan.FromMilliseconds(250)))
                .Where(xs => xs.Count >= 2)
                //.TakeUntilDisable(this) // 게임오브젝트 비활성화 시 스트림 종료
                .Subscribe(
                    xs => Debug.Log("DoubleClick Detected! Count:" + xs.Count), // OnNext
                    _  => Debug.Log("DoubleClick Stream - Error Detected"),     // OnError
                    () => Debug.Log("DoubleClick Stream - Disposed")            // OnCompleted
                );

        // 스트림 종료
        //dbClickStreamDisposable.Dispose();
    }

    private void CheckDoubleClickImmediatley()
    {
        // 목표 : 두 번째 클릭을 인지하는 순간 OnNext 발생

        // 제대로 감지 못하는 버그 존재
        Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0))
            .Buffer(TimeSpan.FromMilliseconds(500), 2)
            .Where(buffer => buffer.Count >= 2)
            .Subscribe(_ => Debug.Log("DoubleClicked!"));
    }

    // 일정 시간 내의 동일한 키보드 연속 2회 이상 입력 후 유지
    private void CheckDoubleKeyPress(KeyCode key, Action action)
    {
        var keyDownStream = 
            Observable.EveryUpdate().Where(_ => Input.GetKeyDown(key));

        var keyUpStream = 
            Observable.EveryUpdate().Where(_ => Input.GetKeyUp(key));

        var keyPressStream = 
            Observable.EveryUpdate().Where(_ => Input.GetKey(key))
                .TakeUntil(keyUpStream);

        var dbKeyStreamDisposable =
            keyDownStream
                .Buffer(keyDownStream.Throttle(TimeSpan.FromMilliseconds(300)))
                .Where(x => x.Count >= 2)
                .SelectMany(_ => keyPressStream)
                .TakeUntilDisable(this)
                .Subscribe(_ => action());
    }

    // 일정 시간 내의 동일한 키보드 연속 2회 이상 입력 후 유지 (마지막 입력으로부터 즉시)
    private void CheckDoubleKeyPressImmediatley(KeyCode key, Action action)
    {
        // TODO : 1회 입력, 2회 입력 유지 이후 액션 분기 구현
        //Subject<bool> keyPressed, doublePressed;

        var keyUpStream = 
            Observable.EveryUpdate().Where(_ => Input.GetKeyUp(key));

        var keyPressStream = 
            Observable.EveryUpdate().Where(_ => Input.GetKey(key));

        var doubleKeyPressStreamDisposable =
            Observable.EveryUpdate().Where(_ => Input.GetKeyDown(key))
                .Buffer(TimeSpan.FromMilliseconds(500), 2)
                .Where(buffer => buffer.Count >= 2)
                .SelectMany(_ => keyPressStream.TakeUntil(keyUpStream))
                .TakeUntilDisable(this)
                .Subscribe(_ => action());

                //.Subscribe(_ => doublePressed.OnNext(true));

        //var keyUpStreamDisposable = 
        //    keyUpStream.Subscribe(_ => keyPressed.OnNext(false));

        //var keyPressStreamStreamDisposable =
        //    keyPressStream.Subscribe(_ => keyPressed.OnNext(true));
    }

    private void EventAsStream()
    {
        var buttonStream =
        _targetButton.onClick.AsObservable()
            //.TakeUntil(_targetButton2.onClick.AsObservable())
            .TakeUntilDestroy(_targetButton)
            .Subscribe(
                _  => Debug.Log("Click!"),
                _  => Debug.Log("Error"),
                () => Debug.Log("Completed")
            );
    }

    private void DragAndDrop()
    {
        this.OnMouseDownAsObservable()
            .SelectMany(_ => this.UpdateAsObservable())
            .TakeUntil(this.OnMouseUpAsObservable())
            .Select(_ => Input.mousePosition)
            .RepeatUntilDestroy(this) // Safe Repeating
            .Subscribe(x => Debug.Log(x));
    }

    private void LifeSpan()
    {
        Observable.Timer(TimeSpan.FromSeconds(3.0))
            .TakeUntilDisable(this)
            .Subscribe(_ => Destroy(gameObject));
    }

    private void ObserveValueChanged()
    {
        // ObserveEveryValueChanged : 클래스 타입에 대해 모두 사용 가능
        this.ObserveEveryValueChanged(x => x._value)
            .Subscribe(x => Debug.Log("Value Changed : " + x));
    }
}