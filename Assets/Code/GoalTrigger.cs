using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    public GameController gameController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ball"))
        {
            // ★ 1. 부딪힌 공에서 GolfBallController 스크립트를 가져와 타수를 확인합니다.
            GolfBallController ball = other.GetComponent<GolfBallController>();
            
            if (ball != null)
            {
                // ★ 2. 유니티 수첩(PlayerPrefs)에 "FinalScore"라는 이름으로 타수를 저장합니다!
                PlayerPrefs.SetInt("FinalScore", ball.ballHitCount);
            }

            // 3. 기존의 씬 전환 코드 실행
            if (gameController != null)
            {
                gameController.OnGoalReached();
            }
        }
    }
}