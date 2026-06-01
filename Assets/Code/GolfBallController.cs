using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(CustomGolfPhysics))] // 물리 스크립트를 필수로 요구함
public class GolfBallController : MonoBehaviour
{
    private CustomGolfPhysics golfBall;
    public CameraTracker cameraTracker;
    private LineRenderer lineRenderer;

    [Header("Shoot Settings")]
    public float power = 0f;
    public float maxPower = 50f;
    public float powerSpeed = 40f;
    
    // 오브젝트를 직접 회전시키지 않고 가상의 조준 각도를 변수로 관리합니다.
    public float aimYaw = 0f;     // 좌우 조준 각도 (A, D)
    public float clubAngle = 12f; // 상하 발사 각도 (1, 2, 3)
    
    [Header("Simulation Settings")]
    public int simulationSteps = 50;
    public float stepTime = 0.05f;

    private bool isCharging = false;
    private bool isPowerIncreasing = true;
    public int ballHitCount = 0;

    void Start()
    {
        // 공에 붙어있는 컴포넌트들을 자동으로 가져옵니다.
        golfBall = GetComponent<CustomGolfPhysics>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (!golfBall.isInAir && golfBall.currentVelocity.magnitude <= 0.05f)
        {
            cameraTracker.isTracking = false; 

            if (!isCharging)
            {
                // A, D 키: 가상의 좌우 각도(aimYaw)만 변경
                aimYaw += Input.GetAxis("Horizontal") * 100f * Time.deltaTime;

                // 1, 2, 3 키: 클럽(상하 각도) 변경
                if (Input.GetKeyDown(KeyCode.Alpha1)) { clubAngle = 12f; }
                if (Input.GetKeyDown(KeyCode.Alpha2)) { clubAngle = 30f; }
                if (Input.GetKeyDown(KeyCode.Alpha3)) { clubAngle = 45f; }

                // 스페이스바 1차 클릭: 게이지 시작
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    isCharging = true;
                    power = 0f;
                    isPowerIncreasing = true;
                }
            }
            else
            {
                // 게이지 핑퐁 연산
                if (isPowerIncreasing)
                {
                    power += powerSpeed * Time.deltaTime;
                    if (power >= maxPower) { power = maxPower; isPowerIncreasing = false; }
                }
                else
                {
                    power -= powerSpeed * Time.deltaTime;
                    if (power <= 0f) { power = 0f; isPowerIncreasing = true; }
                }

                // 스페이스바 2차 클릭: 타격
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    isCharging = false;
                    
                    // Quaternion을 사용해 조준 각도와 클럽 각도가 적용된 최종 발사 벡터 계산
                    Quaternion aimRotation = Quaternion.Euler(-clubAngle, aimYaw, 0);
                    Vector3 shootDirection = aimRotation * Vector3.forward;
                    
                    golfBall.HitBall(shootDirection * power);
                    
                    lineRenderer.positionCount = 0; 
                    cameraTracker.isTracking = true;
                    ballHitCount++;//볼 치는 횟수 카운트 나중에 게임 끝나면 초기화 시켜야함.
                    return; 
                }
            }

            DrawTrajectory();
        }
    }

    void DrawTrajectory()
    {
        Vector3 simulatedPosition = transform.position;
        
        // 궤적 계산용 방향 벡터
        Quaternion aimRotation = Quaternion.Euler(-clubAngle, aimYaw, 0);
        Vector3 shootDirection = aimRotation * Vector3.forward;
        Vector3 simulatedVelocity = (shootDirection * power) / GetComponent<Rigidbody>().mass;

        lineRenderer.positionCount = simulationSteps;
        lineRenderer.SetPosition(0, simulatedPosition);

        for (int i = 1; i < simulationSteps; i++)
        {
            simulatedVelocity.y -= golfBall.gravity * stepTime;
            Vector3 windAcc = golfBall.windForce / GetComponent<Rigidbody>().mass;
            simulatedVelocity += windAcc * stepTime;

            simulatedPosition += simulatedVelocity * stepTime;
            lineRenderer.SetPosition(i, simulatedPosition);

            if (Physics.Raycast(simulatedPosition, simulatedVelocity.normalized, out RaycastHit hit, simulatedVelocity.magnitude * stepTime))
            {
                lineRenderer.positionCount = i + 1;
                lineRenderer.SetPosition(i, hit.point);
                break;
            }
        }
    }
}