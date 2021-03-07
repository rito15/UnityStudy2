using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 설명 : 
public class TestRealtime : MonoBehaviour
{
    [Range(0.001f, 2f)]
    public float _timeScale = 1f;

    public UnityEngine.UI.Text _text0;
    public UnityEngine.UI.Text _text1;
    public UnityEngine.UI.Text _text2;

    private float _time0;
    private float _time1;
    private float _time2;

    void Start()
    {
        StartCoroutine(TimerRoutine0());
        StartCoroutine(TimerRoutine1());
        StartCoroutine(TimerRoutine2());
    }

    private void OnValidate()
    {
        Time.timeScale = _timeScale;
    }

    IEnumerator TimerRoutine0()
    {
        while (true)
        {
            _time0 += Time.deltaTime;
            _text0.text = _time0.ToString("00.##");

            yield return null;
        }
    }

    IEnumerator TimerRoutine1()
    {
        while (true)
        {
            _time1 += 0.001f;
            _text1.text = _time1.ToString("00.##");

            yield return new WaitForSeconds(0.001f);
        }
    }

    IEnumerator TimerRoutine2()
    {
        while (true)
        {
            _time2 += 0.001f;
            _text2.text = _time2.ToString("00.##");

            yield return new WaitForSecondsRealtime(0.001f);
        }
    }
}
