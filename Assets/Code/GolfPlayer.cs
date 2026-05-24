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
            
            // 조준할 때 회전 중심축을 공의 위치와 똑같이 맞춤
            transform.position = golfBall.transform.position; 

            // 파워 및 조준 회전 (A, D 키)
            if (Input.GetKey(KeyCode.W)) power += 10f * Time.deltaTime;
            if (Input.GetKey(KeyCode.S)) power -= 10f * Time.deltaTime;
            transform.Rotate(0, Input.GetAxis("Horizontal") * 100f * Time.deltaTime, 0); 

            power = Mathf.Clamp(power, 0f, 50f); // 파워 제한

            // ★ [추가된 부분] 숫자 키로 클럽(발사 각도) 선택 ★
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                angle = 12f; // 드라이버 (1W): 각도가 낮고 멀리 감
                Debug.Log("드라이버 선택됨 (12도)");
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                angle = 30f; // 7번 아이언 (7I): 중간 각도
                Debug.Log("아이언 선택됨 (30도)");
            }
            if (Input.GetKeyDown(KeyCode.Alpha3)) {
                angle = 45f; // 피칭 웨지 (PW): 높이 뜨고 짧게 감
                Debug.Log("웨지 선택됨 (45도)");
            }

            // 바람이 반영된 궤적 예측선 그리기
            DrawTrajectory();

            // 스페이스바로 타격
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // angle 값에 따라 Sin(Y축), Cos(앞방향) 비율이 달라지며 포물선이 변함
                Vector3 shootDirection = transform.forward * Mathf.Cos(angle * Mathf.Deg2Rad) + transform.up * Mathf.Sin(angle * Mathf.Deg2Rad);
                golfBall.HitBall(shootDirection * power);
                
                GetComponent<LineRenderer>().positionCount = 0; // 발사 후 궤적 숨김
                cameraTracker.isTracking = true; // 트래킹 카메라로 전환
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