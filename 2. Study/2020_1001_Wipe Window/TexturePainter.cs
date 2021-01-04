using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 2020. 10. 01.
// Plane 텍스쳐에 마우스 클릭으로 그림 그리기
public class TexturePainter : MonoBehaviour
{
    /* [Note]

       1. 텍스쳐 Import Settings
        - Format : RGBA 32bit
        - Advanced -> Read/Write Enabled (Check)

       2. 대상 게임오브젝트
        - Plane에 사용할 것
        - Position 상관 없음
        - Rotation : (90, 180, 0)
        - Scale    : (1, 1, 1)

       3. 오버레이 카메라 이용
        - "Window" 레이어 준비

        - 오버레이 카메라와 Plane을 다른 위치에 따로 준비
        - 오버레이 카메라의 Render Type을 Overlay로 설정
        - 오버레이 카메라의 Culling Mask를 Window로만 설정

        - Plane을 수직으로 비추는 Directional Light 준비
        - 라이트의 Culling Mask를 Window로만 설정

        - Plane의 레이어를 Window로 설정

        - 메인 카메라에서 Stack을 추가하여, 해당 Overlay 카메라 등록

    */

    // Plane만 렌더링할 카메라
    public Camera _overlayCam;

    public int _brushRadius = 50;
    public Color _brushColor = Color.white;
    public float _alpha = 0.2f;

    private Texture2D _tex;
    private Material _material;
    private Vector2Int _clickedPixelPoint;

    private void Start()
    {
        DupAndInitMainTexture();

        if (_overlayCam == null)
        {
            Debug.Log("TexturePainter : Overlay 카메라가 등록되지 않아 메인 카메라로 대체합니다.");
            _overlayCam = Camera.main;
        }

        //Debug.Log(_tex.width);
        //Debug.Log(_tex.height);
    }

    private void Update()
    {
        Vector3 raycastPoint;

        if (Input.GetMouseButton(0))
        {
            // 레이캐스트 지점 픽셀 포인트 잡아오기
            raycastPoint = GetRaycastPoint();
            _clickedPixelPoint = GetPixelPoint(raycastPoint);

            //Debug.Log(_clickedPixelPoint);

            if (!InPixelRange(_clickedPixelPoint))
                return;

            // 원형 픽셀 지점들 구해오기
            var targetPixels = GetCircleShapeTargetPixelPoints(_clickedPixelPoint, _brushRadius);
            //Debug.Log(targetPixels.Count);

            // 픽셀들에 그려주기
            targetPixels.ForEach(point =>
            {
                _tex.SetPixel(point.x, point.y, new Color(_brushColor.r, _brushColor.g, _brushColor.b, _alpha));
            });

            _tex.Apply(true);
        }
    }

    private void DupAndInitMainTexture()
    {
        _material = GetComponent<Renderer>().material;
        //_tex = _material.mainTexture as Texture2D;

        _tex = Instantiate(_material.mainTexture) as Texture2D;
        _material.mainTexture = _tex;
    }

    // 마우스 클릭 지점 레이캐스트하여 월드 좌표 구하기
    private Vector3 GetRaycastPoint()
    {
        if (Physics.Raycast(_overlayCam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1000f, -1))
            return hit.point;

        //Debug.Log("Unvalid Raycast");
        return Vector3.negativeInfinity;
    }

    // 월드 좌표 -> Texel 좌표 변환하여 구해오기
    private Vector2Int GetPixelPoint(Vector3 raycastPoint)
    {
        if (Vector3.Magnitude(raycastPoint - Vector3.negativeInfinity) < 1f)
            return new Vector2Int(-1, -1);

        Vector2 temp = raycastPoint;
        temp -= (Vector2)transform.position;
        temp += Vector2.one * 5f;

        // width * height 계산
        temp = new Vector2(temp.x * _tex.width * 0.1f, temp.y * _tex.height * 0.1f);

        return new Vector2Int((int)temp.x, (int)temp.y);
    }

    // 원형의 픽셀 타겟들 구해오기
    private List<Vector2Int> GetCircleShapeTargetPixelPoints(Vector2Int pixelPoint, int pixelRadius)
    {
        (int x, int y) point = ((int)pixelPoint.x, (int)pixelPoint.y);
        List<Vector2Int> resultList = new List<Vector2Int>();

        for (int x = point.x - pixelRadius; x < point.x + pixelRadius; x++)
        {
            for (int y = point.y - pixelRadius; y < point.y + pixelRadius; y++)
            {
                if (InPixelRange(x, y) && Vector2.Distance(pixelPoint, new Vector2(x, y)) <= pixelRadius)
                    resultList.Add(new Vector2Int(x, y));
            }
        }

        return resultList;
    }

    private bool InPixelRange(int xPoint, int yPoint)
    {
        return (xPoint >= 0 && yPoint >= 0) &&
               (xPoint <= _tex.width && yPoint <= _tex.height);
    }

    private bool InPixelRange(Vector2Int pixelPoint)
    {
        return InPixelRange(pixelPoint.x, pixelPoint.y);
    }
}
