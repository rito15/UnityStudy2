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
    public float _intValue;

    public bool _boolValue;

    public IntReactiveProperty _intProp = new IntReactiveProperty(0);
    public ReactiveProperty<int> _intProp2 = new ReactiveProperty<int>(1);

    private void Start()
    {
        //EventAsStream();
        //DragAndDrop();
        //LifeSpan();

        //CheckDoubleClickImmediatley();
        //CheckDoubleKeyPress(KeyCode.A, () => transform.Translate(Vector3.left * Time.deltaTime * 5f));
        //CheckDoubleKeyPress(KeyCode.D, () => transform.Translate(Vector3.right * Time.deltaTime * 5f));
        //CheckDoubleKeyPressImmediatley(KeyCode.W, () => Debug.Log("WW"));

        //ObserveValueChanged();
        //AsyncGetFromWeb();

        //RotateObjectWithDrag();

        //UpdateDifferences();
        //CaptureWhenValueChanges();
        //RefineRapidlyChangingValue();

        SubjectTest();
    }

    private void UpdateDifferences()
    {
        // 게임오브젝트가 활성화된 동안에만 OnNext() 통지
        // 게임오브젝트가 파괴될 때 OnCompleted()
        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButtonDown(0))
            .Subscribe(
                _ => Debug.Log("UpdateAsObservable" + this._intValue),
                () => Debug.Log("UpdateAsObservable Completed")
            );

        // 대상 객체가 활성화된 동안에만 OnNext() 통지
        // 대상 객체가 파괴될 때 OnCompleted()
        this.ObserveEveryValueChanged(_ => Input.GetMouseButton(0))
            .Where(x => x)
            .Skip(TimeSpan.Zero)
            .Subscribe(
                _ => Debug.Log("ObserveEveryValueChanged"), 
                () => Debug.Log("ObserveEveryValueChanged Completed")
            );

        // 독자적
        Observable.EveryUpdate()
            .Where(_ => Input.GetMouseButtonDown(1))
            .Subscribe(
                _ => Debug.Log("EveryUpdate" + this._intValue),
                () => Debug.Log("EveryUpdate Completed")
            );
    }

    private void AsyncGetFromWeb()
    {
        // Obsolete : Use UnityEngine.Networking.UnityWebRequest Instead.
        ObservableWWW.Get("http://google.co.kr/")
            .Subscribe(
                x => Debug.Log(x.Substring(0, 20)), // onSuccess
                ex => Debug.LogException(ex)       // onError
            );
    }

    private void WhenAllExample()
    {
        var parallel = Observable.WhenAll(
            ObservableWWW.Get("http://google.com/"),
            ObservableWWW.Get("http://bing.com/"),
            ObservableWWW.Get("http://unity3d.com/")
        );

        parallel.Subscribe(xs =>
        {
            Debug.Log(xs[0].Substring(0, 100)); // google
            Debug.Log(xs[1].Substring(0, 100)); // bing
            Debug.Log(xs[2].Substring(0, 100)); // unity
        });
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

    private void RotateObjectWithDrag()
    {
        float rotSpeed = 500f;

        // 오브젝트(콜라이더)에 마우스 클릭하여 드래그하면 마우스 이동거리에 따라 회전
        // 마우스 떼면 스트림 종료 및 다시 시작(Repeat)

        this.UpdateAsObservable()
            .SkipUntil(this.OnMouseDownAsObservable())
            .Select(_ =>
                new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"))
            )
            .TakeUntil(this.OnMouseUpAsObservable())
            .RepeatUntilDisable(this)
            .Subscribe(move =>
            {
                transform.rotation = 
                    Quaternion.AngleAxis(move.y * rotSpeed * Time.deltaTime, Vector3.right) *
                    Quaternion.AngleAxis(-move.x * rotSpeed * Time.deltaTime, Vector3.up) *
                    transform.rotation;
            });



    }
    private void LifeSpan()
    {
        Observable.Timer(TimeSpan.FromSeconds(3.0))
            .TakeUntilDisable(this)
            .Subscribe(_ => Destroy(gameObject));
    }

    // 값이 변하는 순간을 포착
    private void CaptureWhenValueChanges()
    {
        // 마우스 클릭, 떼는 순간 모두 포착
        this.UpdateAsObservable()
            .Select(_ => Input.GetMouseButton(0))
            .DistinctUntilChanged()
            .Skip(1) // 시작하자마자 false값에 대한 판정 때문에 "Up" 호출되는 것 방지
            .Subscribe(down =>
            {
                if (down)
                    Debug.Log($"Down : {Time.frameCount}");
                else
                    Debug.Log($"Up : {Time.frameCount}");
            });


        // 값이 false -> true로 바뀌는 순간만 포착
        this.UpdateAsObservable()
            .Select(_ => this._boolValue)
            .DistinctUntilChanged()
            .Where(x => x)
            .Skip(TimeSpan.Zero) // 초기값이 true일 때 첫 프레임에 바로 호출되는 것 방지
            .Subscribe(_ => Debug.Log("TRUE"));

        // 매 프레임, 값의 변화 포착만을 위한 간단한 구문 (위 구문을 간소화)
        this.ObserveEveryValueChanged(_ => this._boolValue)
            .Where(x => x)
            .Skip(TimeSpan.Zero)
            .Subscribe(_ => Debug.Log("TRUE 2"));
    }

    private void TestObserveValueChanged()
    {
        // ObserveEveryValueChanged : 클래스 타입에 대해 모두 사용 가능
        this.ObserveEveryValueChanged(x => x._intValue)
            .Subscribe(x => Debug.Log("Value Changed : " + x));
    }

    private void TestObserveValueOnEveryUpdate()
    {
        // 대상의 값을 그냥 매 프레임 조건 없이 출력
        this.UpdateAsObservable()
            .Select(_ => this._intValue)
            .Subscribe(x => Debug.Log(x));
    }


    public bool _isTouched = false;
    public bool _isTouchedRefined = false;
    private bool _isGroundedRefined = false;
    private void OnTriggerEnter(Collider other) => _isTouched = true;
    private void OnTriggerExit(Collider other) => _isTouched = false;

    // 급변하는 값 정제하기
    private void RefineRapidlyChangingValue()
    {
        // ObserveEveryValueChanged : 값이 변화했을 때 통지
        // ThrottleFrame(5) : 마지막 통지로부터 5프레임동안 값의 통지를 받지 않으면 OnNext()

        // 따라서 값이 급변하는 동안에는 OnNext() 하지 않고
        // 5프레임 이내로 값이 변하지 않았을 때 마지막으로 기억하는 값을 전달

        // 5프레임 이내에서 순간적으로 급변하는 값들을 무시하여, 값을 정제하는 효과가 있음

        // 사용 예시 : 닿았는지 여부 검사, 비탈길에서의 isGrounded 검사

        this.ObserveEveryValueChanged(_ => this._isTouched)
            .ThrottleFrame(5)
            .Subscribe(x => _isTouchedRefined = x);

        TryGetComponent(out CharacterController cc);
        cc.UpdateAsObservable()
            .Select(_ => cc.isGrounded)
            .DistinctUntilChanged()
            .ThrottleFrame(5)
            .Subscribe(x => _isGroundedRefined = x);
    }

    private void SubjectTest()
    {
        Subject<string> strSubject = new Subject<string>();

        var disposable = 
            strSubject
            .Subscribe(str => Debug.Log("Next : " + str), () => Debug.Log("End1"));

        strSubject
            .DelayFrame(10)
            .Subscribe(str => Debug.Log("Delayed Next : " + str), () => Debug.Log("End2"));

        strSubject.OnNext("A");

        disposable.Dispose();

        strSubject.OnNext("B");
        strSubject.OnCompleted();

        strSubject.OnNext("C");
    }
}