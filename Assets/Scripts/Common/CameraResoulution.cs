using UnityEngine;

public class CameraResoulution : MonoBehaviour 
{
    private void Awake()
    {
        // 우리가 원하는 고정 비율 (9:16)
        // 1080 / 1920 = 0.5625
        float targetAspect = 9.0f / 16.0f;

        // 현재 기기의 화면 비율
        float windowAspect = (float)Screen.width / Screen.height;

        // 비율 차이 계산
        float scaleHeight = windowAspect / targetAspect;

        Camera camera = GetComponent<Camera>();

        // 1. 기기가 더 '홀쭉한' 경우 (세로로 긴 경우, 예: 최신 갤럭시 20:9 등)
        // -> 위아래에 레터박스(검은 여백)를 만듦
        if (scaleHeight < 1.0f)
        {
            Rect rect = camera.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f; // 중앙 정렬

            camera.rect = rect;
        }
        // 2. 기기가 더 '뚱뚱한' 경우 (가로로 넓은 경우, 예: 아이패드 4:3)
        // -> 좌우에 필러박스(검은 여백)를 만듦
        else
        {
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = camera.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f; // 중앙 정렬
            rect.y = 0;

            camera.rect = rect;
        }
    }
}