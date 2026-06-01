using UnityEngine;
using UnityEngine.UI;
using TMPro; // ★ TextMeshPro를 사용하기 위해 반드시 추가해야 합니다!
public class WindUIController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("바람 물리 엔진이 들어있는 스크립트를 연결해주세요")]
    public CustomGolfPhysics golfPhysics;

    [Header("3D Model")]
    public Transform windArrow3D;

    [Header("UI Text")]
    public TextMeshProUGUI windSpeedText; // ★ TMP 전용 변수로 변경!

    void Update()
    {
        // 골프 물리 스크립트가 연결되어 있다면 매 프레임 UI를 동기화합니다.
        if (golfPhysics != null)
        {
            SyncWindUI(golfPhysics.windForce);
        }
    }

    private void SyncWindUI(Vector3 currentWind)
    {
        float speed = currentWind.magnitude;

        // 바람이 0이 아닐 때만 회전 (0일 때 에러 방지)
        if (windArrow3D != null && currentWind != Vector3.zero)
        {
            // ★ 핵심 해결 코드 ★
            // "이 모델의 기본 방향(Vector3.up)을, 바람의 방향(currentWind)으로 맞춰라!"
            windArrow3D.localRotation = Quaternion.FromToRotation(Vector3.up, currentWind);
        }

        if (windSpeedText != null)
        {
            windSpeedText.text = speed.ToString("F1") + "m";
        }
    }
}