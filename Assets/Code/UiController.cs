using UnityEngine;
using UnityEngine.UI; // UI 컴포넌트(Image 등)를 제어하기 위해 필수입니다!

public class UiController : MonoBehaviour
{
    [Header("References")]
    public GolfPlayer golfPlayer;
    public Image powerBarImage; // 방금 설정한 게이지 바 UI 이미지를 연결할 곳

    [Header("Settings")]
    public float powerMax = 15f;
    public float chargeSpeed = 2f; // 게이지가 오르내리는 속도

    private float currentPower = 0f; // 0.0 ~ 1.0 사이의 비율 값
    private int direction = 1;       // 1이면 상승, -1이면 하락
    
    private bool isCharging = false;
    private bool isPowerLocked = false;

    void Update()
    {
        HandleInput();
        UpdateChargingUI();
    }

    private void HandleInput()
    {
        // 스페이스바 입력 처리
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isCharging && !isPowerLocked)
            {
                // [첫 번째 스페이스바] 차징 시작
                isCharging = true;
                currentPower = 0f;
                direction = 1;
                Debug.Log("차징 시작!");
            }
            else if (isCharging)
            {
                // [두 번째 스페이스바] 차징 확정
                isCharging = false;
                isPowerLocked = true;

                // GolfPlayer에게 최종 계산된 힘(현재 비율 * 최대 파워)을 전달합니다.
                if (golfPlayer != null)
                {
                    golfPlayer.power = currentPower * powerMax;
                    Debug.Log($"파워 확정! 골프 플레이어에게 전달된 힘: {golfPlayer.power}");
                    golfPlayer.hit = true; // 골프 플레이어에게 타격 신호 전달
                }
            }
            else if (isPowerLocked)
            {
                // (선택 사항) 세 번째 누르거나, 공을 친 후 게이지 초기화
                isPowerLocked = false;
                currentPower = 0f;
                powerBarImage.fillAmount = 0f;
                Debug.Log("UI 게이지 초기화");
            }
        }
    }

    private void UpdateChargingUI()
    {
        // 차징 중일 때만 게이지 바 수치를 변경합니다.
        if (isCharging)
        {
            // 시간에 따라 currentPower 증가 또는 감소
            currentPower += direction * chargeSpeed * Time.deltaTime;

            // 핑퐁(PingPong) 로직: 100%(1.0)에 도달하면 깎이고, 0%(0.0)에 도달하면 다시 오름
            if (currentPower >= 1f)
            {
                currentPower = 1f;
                direction = -1; // 하락 방향으로 전환
            }
            else if (currentPower <= 0f)
            {
                currentPower = 0f;
                direction = 1;  // 상승 방향으로 전환
            }

            // 계산된 0.0 ~ 1.0 사이의 값을 UI 이미지의 Fill Amount에 바로 적용
            if (powerBarImage != null)
            {
                powerBarImage.fillAmount = currentPower;
            }
        }
    }
}