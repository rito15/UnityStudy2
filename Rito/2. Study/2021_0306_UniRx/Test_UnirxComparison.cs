using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-03-14 PM 9:03:10
// 작성자 : Rito

public class Test_UnirxComparison : MonoBehaviour
{
    /// <summary> 키보드 연속 입력 및 유지 감지 </summary>
    private class KeyForDoublePressDetection
    {
        public KeyCode Key { get; private set; }

        /// <summary> 한 번 눌러서 유지 </summary>
        public bool SinglePressed { get; private set; }

        /// <summary> 두 번 눌러서 유지 </summary>
        public bool DoublePressed { get; private set; }

        private bool doublePressDetected;
        private float doublePressThreshold;
        private float lastKeyDownTime;

        public KeyForDoublePressDetection(KeyCode key, float threshold = 0.3f)
        {
            this.Key = key;
            SinglePressed = false;
            DoublePressed = false;
            doublePressDetected = false;
            doublePressThreshold = threshold;
            lastKeyDownTime = 0f;
        }

        public void ChangeKey(KeyCode key)
        {
            this.Key = key;
        }
        public void ChangeThreshold(float seconds)
        {
            doublePressThreshold = seconds > 0f ? seconds : 0f;
        }

        /// <summary> MonoBehaviour.Update()에서 호출 : 키 정보 업데이트 </summary>
        public void UpdateCheck()
        {
            if (Input.GetKeyDown(Key))
            {
                doublePressDetected =
                    (Time.time - lastKeyDownTime < doublePressThreshold);

                lastKeyDownTime = Time.time;
            }

            if (Input.GetKey(Key))
            {
                if (doublePressDetected)
                    DoublePressed = true;
                else
                    SinglePressed = true;
            }
            else
            {
                doublePressDetected = false;
                DoublePressed = false;
                SinglePressed = false;
            }
        }

        /// <summary> MonoBehaviour.Update()에서 호출 : 키 입력에 따른 동작 </summary>
        public void UpdateAction(Action singlePressAction, Action doublePressAction)
        {
            if(SinglePressed) singlePressAction?.Invoke();
            if(DoublePressed) doublePressAction?.Invoke();
        }
    }

    private KeyForDoublePressDetection[] keys;

    private void Start()
    {
        keys = new[]
        {
            new KeyForDoublePressDetection(KeyCode.W),
            new KeyForDoublePressDetection(KeyCode.A),
            new KeyForDoublePressDetection(KeyCode.S),
            new KeyForDoublePressDetection(KeyCode.D),
        };
    }

    private void Update()
    {
        for (int i = 0; i < keys.Length; i++)
        {
            keys[i].UpdateCheck();
        }

        keys[0].UpdateAction(() => Debug.Log("W"), () => Debug.Log("WW"));
        keys[1].UpdateAction(() => Debug.Log("A"), () => Debug.Log("AA"));
        keys[2].UpdateAction(() => Debug.Log("S"), () => Debug.Log("SS"));
        keys[3].UpdateAction(() => Debug.Log("D"), () => Debug.Log("DD"));
    }
    
}