using UnityEngine;
using UnityEngine.UI;
using TMPro; // ★ TextMeshPro를 사용하기 위해 반드시 추가해야 합니다!

public class UiController : MonoBehaviour
{
    [Header("References")]
    public GolfBallController golfPlayer; 
    public CustomGolfPhysics golfPhysics;
    public Image powerBarImage;
    public TextMeshProUGUI hitCountText;
    public TextMeshProUGUI cofficientText;
    void Update()
    {
        if (golfPlayer == null || powerBarImage == null || golfPhysics == null ) 
        {
            Debug.LogWarning("UI 컨트롤러에 골프공, physics이나 이미지가 연결되지 않았습니다!");
            return;
        }
        hitWriteText();
        coefficientWriteText();
    }
    void coefficientWriteText()
    {
        cofficientText.text = $"Friction Coefficient : {golfPhysics.currentFriction:F2}\nRestitution Coefficient : {golfPhysics.currentRestitution:F2}";
    }
    void hitWriteText()
    {
        // 비율 계산
        float powerRatio = golfPlayer.power / golfPlayer.maxPower;

        // UI에 적용
        powerBarImage.fillAmount = powerRatio;
        // hit text
        hitCountText.text = $"Stroke :{golfPlayer.ballHitCount}";
        // ★ [테스트용 로그] 이 메시지가 콘솔 창에 다다다닥 떠야 정상입니다.
        if (powerRatio > 0)
        {
            Debug.Log("UI 게이지 채우는 중... 현재 비율: " + powerRatio);
        }
    }
}