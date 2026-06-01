using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [Header("설정")]
    public string nextSceneName = "GameOver";

    // 블록이나 공이 부딪혔을 때 호출할 수 있도록 반드시 'public'으로 만듭니다.
    public void OnGoalReached()
    {
        Debug.Log("Goal 블록에 공이 닿았습니다! 씬을 전환합니다.");
        SceneManager.LoadScene(nextSceneName);
    }
}