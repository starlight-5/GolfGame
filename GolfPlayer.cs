using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GolfPlayer : MonoBehaviour
{
    public CustomGolfPhysics golfBall;
    public CameraTracker cameraTracker;
    private LineRenderer lineRenderer;

    [Header("Shoot Settings")]
    public float power = 15f;
    public float angle = 30f;
    public int simulationSteps = 50; // 예측선 길이
    public float stepTime = 0.05f;   // 시뮬레이션 간격
    public bool hit = false;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (!golfBall.isInAir && golfBall.currentVelocity.magnitude == 0)
        {
            cameraTracker.isTracking = false; // 조준 모드

            // 파워 및 조준 회전
            if (Input.GetKey(KeyCode.W)) power += 10f * Time.deltaTime;
            if (Input.GetKey(KeyCode.S)) power -= 10f * Time.deltaTime;
            transform.Rotate(0, Input.GetAxis("Horizontal") * 50f * Time.deltaTime, 0);

            power = Mathf.Clamp(power, 0f, 50f);

            // 5. 바람이 반영된 궤적 예측선 그리기
            DrawTrajectory();

            // 스페이스바로 타격
                if (hit)
                {
                    Vector3 shootDirection = transform.forward * Mathf.Cos(angle * Mathf.Deg2Rad) + transform.up * Mathf.Sin(angle * Mathf.Deg2Rad);
                    golfBall.HitBall(shootDirection * power);

                    lineRenderer.positionCount = 0; // 발사 후 궤적 숨김
                    cameraTracker.isTracking = true; // 트래킹 카메라로 전환
                    hit = false; // 타격 신호 초기화
                }
            
        }
    }

    void DrawTrajectory()
    {
        Vector3 simulatedPosition = golfBall.transform.position;
        Vector3 shootDirection = transform.forward * Mathf.Cos(angle * Mathf.Deg2Rad) + transform.up * Mathf.Sin(angle * Mathf.Deg2Rad);

        // 질량을 고려한 초기 속도 예측
        Vector3 simulatedVelocity = (shootDirection * power) / golfBall.GetComponent<Rigidbody>().mass;

        lineRenderer.positionCount = simulationSteps;
        lineRenderer.SetPosition(0, simulatedPosition);

        for (int i = 1; i < simulationSteps; i++)
        {
            // 중력 및 바람 시뮬레이션
            simulatedVelocity.y -= golfBall.gravity * stepTime;
            Vector3 windAcc = golfBall.windForce / golfBall.GetComponent<Rigidbody>().mass;
            simulatedVelocity += windAcc * stepTime;

            simulatedPosition += simulatedVelocity * stepTime;
            lineRenderer.SetPosition(i, simulatedPosition);

            // 지형 충돌 가상 처리 (Raycast)
            if (Physics.Raycast(simulatedPosition, simulatedVelocity.normalized, out RaycastHit hit, simulatedVelocity.magnitude * stepTime))
            {
                lineRenderer.positionCount = i + 1;
                lineRenderer.SetPosition(i, hit.point);
                break;
            }
        }
    }
}