using UnityEngine;
using UnityEngine.UI; 

public class UiController : MonoBehaviour
{
    [Header("References")]
    public GolfPlayer golfPlayer;
    public Image powerBarImage; 
    public Image yellowGage;

    [Header("Settings")]
    public float powerMax = 15f;
    
    // chargeSpeed 대신 '몇 초마다 한 칸씩 움직일지' 결정하는 변수로 바꿨습니다.
    [Tooltip("몇 초마다 게이지가 한 칸(0.125)씩 움직일지 설정 (예: 0.1초)")]
    public float stepDelay = 0.1f; 

    private float currentPower = 0f; 
    private int direction = 1;       
    
    // ★ 타이머 역할을 할 변수 추가
    private float timer = 0f;

    private bool isCharging = false;
    private bool isPowerLocked = false;
    public bool isChangeYellowGage = false;

    void Update()
    {
        YellowGageRange();
        HandleInput();
        UpdateChargingUI();
    }

    private void YellowGageRange()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (yellowGage.fillAmount < 1f)
            {
                yellowGage.fillAmount += 0.125f;
                Debug.Log("예측게이지 증가");
            }
            else
            {
                yellowGage.fillAmount = 1f;
            }
            isChangeYellowGage = true;
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (yellowGage.fillAmount > 0f)
            {
                yellowGage.fillAmount -= 0.125f;
                Debug.Log("예측게이지 감소");
            }
            else
            {
                yellowGage.fillAmount = 0f;
            }
            isChangeYellowGage = true;
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isCharging && !isPowerLocked)
            {
                isCharging = true;
                currentPower = 0f;
                direction = 1;
                timer = 0f; // ★ 차징 시작 시 타이머도 0으로 초기화
                Debug.Log("차징 시작!");
            }
            else if (isCharging)
            {
                isCharging = false;
                isPowerLocked = true;

                if (golfPlayer != null)
                {
                    golfPlayer.power = currentPower * powerMax;
                    Debug.Log($"파워 확정! 골프 플레이어에게 전달된 힘: {golfPlayer.power}");
                    golfPlayer.hit = true; 
                }
            }
            else if (isPowerLocked)
            {
                isPowerLocked = false;
                currentPower = 0f;
                powerBarImage.fillAmount = 0f;
                Debug.Log("UI 게이지 초기화");
            }
        }
    }

    private void UpdateChargingUI()
    {
        if (isCharging)
        {
            // ★ 매 프레임마다 시간을 누적합니다.
            timer += Time.deltaTime;

            // ★ 누적된 시간이 우리가 설정한 딜레이(stepDelay)를 넘었을 때만 게이지를 0.125 올립니다.
            if (timer >= stepDelay)
            {
                // 다음 칸을 위해 타이머를 다시 0으로 돌려놓습니다.
                timer = 0f; 

                // 게이지 한 칸(0.125) 이동!
                currentPower += direction * 0.125f;

                if (currentPower >= 1f)
                {
                    currentPower = 1f;
                    direction = -1; 
                }
                else if (currentPower <= 0f)
                {
                    currentPower = 0f;
                    direction = 1;  
                }

                if (powerBarImage != null)
                {
                    powerBarImage.fillAmount = currentPower;
                }
            }
        }
    }
}