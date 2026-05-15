using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 플레이어의 현재 상태를 정의합니다.
    public enum PlayerState { Aiming, PowerSetting, ImpactSetting, Swinging, Waiting }
    
    [Header("Player State")]
    public PlayerState currentState = PlayerState.Aiming;

    [Header("Managers & Targets")]
    public BallPhysics targetBall;   // 타격할 공 오브젝트
    public CamManager camManager;    // 우리가 만든 카메라 감독님

    [Header("Club Settings")]
    public float selectedClubAngle = 30f; // 7번 아이언의 대략적인 발사각

    [Header("Hit Data (UI 게이지 대체용)")]
    private float currentPower = 0f;
    private float currentAccuracy = 0f;

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // 스페이스바를 누를 때마다 골프 스윙 단계가 진행됩니다.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            switch (currentState)
            {
                case PlayerState.Aiming:
                    // 1. 조준 완료 -> 파워 게이지 멈추기 대기
                    currentState = PlayerState.PowerSetting;
                    Debug.Log("파워 설정 시작!");
                    break;

                case PlayerState.PowerSetting:
                    // 2. 파워 결정 -> 팡야(임팩트) 맞추기 대기
                    currentPower = Random.Range(80f, 100f); // 프로토타입용 임시 랜덤 파워
                    currentState = PlayerState.ImpactSetting;
                    Debug.Log($"파워 결정: {currentPower}. 임팩트(팡야) 집중!");
                    
                    // 🎬 연출 1: 파워를 결정하는 순간, 공과 클럽에 집중하는 줌인 카메라로 전환!
                    if (camManager != null) camManager.SetImpactView(); 
                    break;

                case PlayerState.ImpactSetting:
                    // 3. 임팩트 타이밍 결정 -> 타격!
                    currentAccuracy = Random.Range(-5f, 5f); // 0에 가까울수록 Perfect(팡야)
                    currentState = PlayerState.Swinging;
                    Debug.Log($"임팩트 정확도: {currentAccuracy}. 스윙 시작!");
                    
                    // ※ 실제 게임에서는 애니메이션을 재생하고, 채가 공에 닿는 순간 ExecuteHit()이 실행되어야 합니다.
                    // 현재는 애니메이션이 없으므로 스페이스바를 누르자마자 바로 타격되게 호출합니다.
                    ExecuteHit(); 
                    break;
            }
        }
    }

    // 실제로 공에 물리적인 힘을 전달하는 함수
    public void ExecuteHit()
    {
        // 스윙 상태가 아니거나 공이 없으면 취소
        if (currentState != PlayerState.Swinging || targetBall == null) return;

        // 플레이어가 바라보는 앞방향을 타격 방향으로 설정
        Vector3 hitDirection = transform.forward;

        // 공에게 전달할 데이터 꾸러미 생성
        HitData hitData = new HitData(
            power: currentPower,
            accuracy: currentAccuracy,
            direction: hitDirection,
            launchAngle: selectedClubAngle,
            spin: 0f
        );

        // 공 발사 명령!
        targetBall.ApplyHit(hitData);

        // 플레이어는 공이 날아가는 걸 지켜보는 대기 상태로 전환
        currentState = PlayerState.Waiting;
        
        // 🎬 연출 2: 타격 직후, 하늘로 날아가는 공을 쫓아가는 추적 카메라로 전환!
        if (camManager != null) camManager.SetFlightTrackingView(); 
    }
}