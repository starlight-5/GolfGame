using UnityEngine;
using TMPro; // TextMeshPro 사용
using UnityEngine.SceneManagement; // 씬 전환 기능 사용

public class GameOverSceneManager : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI finalScoreText;

    void Start()
    {
        // 1. 씬이 시작되자마자 수첩(PlayerPrefs)에 적어둔 타수를 꺼내옵니다. (저장된 게 없으면 0)
        int score = PlayerPrefs.GetInt("FinalScore", 0);

        // 2. 화면의 UI 텍스트에 예쁘게 표시합니다.
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Stroke : {score}";
        }
    }

    // ★ 버튼을 클릭했을 때 실행될 함수입니다. (반드시 public이어야 버튼이 찾을 수 있습니다)
    public void OnClickRestartButton()
    {
        Debug.Log("다시 시작 버튼 클릭! SampleScene으로 돌아갑니다.");
        
        // 다시 원래 게임 씬으로 돌아갑니다.
        SceneManager.LoadScene("SampleScene");
    }
}