using UnityEngine;

public class StickSwing : MonoBehaviour
{
    [Header("Swing Settings")]
    public float maxBackswingAngle = -90f; 
    public float hitAngle = 0f;            
    public float chargeSpeed = 30f;        
    public float swingSpeedMultiplier = 5f;

    [Tooltip("3D 스윙 회전축 (보통 X축(1,0,0) 또는 Z축(0,0,1)을 사용합니다)")]
    public Vector3 swingAxis = new Vector3(1, 0, 0); // X축을 기본값으로 설정

    [Header("References")]
    public Rigidbody ballRigidbody;
    public float maxHitForce = 1000f;

    [Tooltip("공이 날아갈 방향 (X: 좌우, Y: 위아래, Z: 앞뒤)")]
    public Vector3 hitDirection = new Vector3(0, 1, 1); // 기본값: 앞(Z)으로 날아가며 위(Y)로 뜸

    private enum SwingState { Idle, Charging, Swinging, FollowThrough }
    private SwingState currentState = SwingState.Idle;

    private float currentPower = 0f; 
    private float currentAngle = 0f;
    
    // 시작할 때의 기본 회전값을 저장하여 플레이어가 회전해도 스윙이 꼬이지 않게 함
    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        HandleInput();
        ExecuteState();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0) && currentState == SwingState.Idle)
        {
            currentState = SwingState.Charging;
            currentPower = 0f;
        }
        
        if (Input.GetMouseButtonUp(1) && currentState == SwingState.Charging)
        {
            currentState = SwingState.Swinging;
        }
    }

    private void ExecuteState()
    {
        switch (currentState)
        {
            case SwingState.Charging:
                currentPower += (chargeSpeed / Mathf.Abs(maxBackswingAngle)) * Time.deltaTime;
                currentPower = Mathf.Clamp01(currentPower);
                
                currentAngle = Mathf.Lerp(0, maxBackswingAngle, currentPower);
                ApplyRotation();
                break;

            case SwingState.Swinging:
                float speed = swingSpeedMultiplier * (1f + currentPower);
                currentAngle = Mathf.LerpAngle(currentAngle, hitAngle, Time.deltaTime * speed);
                ApplyRotation();

                if (Mathf.Abs(Mathf.DeltaAngle(currentAngle, hitAngle)) < 2f)
                {
                    HitBall();
                    currentState = SwingState.FollowThrough;
                }
                break;

            case SwingState.FollowThrough:
                currentAngle = Mathf.LerpAngle(currentAngle, 45f, Time.deltaTime * 2f);
                ApplyRotation();
                
                if (Input.GetMouseButtonDown(0)) 
                {
                    currentState = SwingState.Idle;
                    currentAngle = 0f;
                    ApplyRotation();
                }
                break;
        }
    }

    // Quaternion.Euler 대신 AngleAxis를 사용하여 원하는 축으로 회전시킵니다.
    private void ApplyRotation()
    {
        // localRotation을 사용하여 부모(플레이어)가 회전해도 스윙 궤적이 유지되도록 합니다.
        transform.localRotation = initialRotation * Quaternion.AngleAxis(currentAngle, swingAxis.normalized);
    }

    private void HitBall()
    {
        if (ballRigidbody != null)
        {
            float finalForce = maxHitForce * currentPower;
            
            // 플레이어가 바라보는 방향(transform.forward)을 기준으로 공을 날릴 수도 있습니다.
            // 여기서는 인스펙터에서 설정한 hitDirection 방향으로 날립니다.
            Vector3 finalDirection = hitDirection.normalized; 
            
            ballRigidbody.AddForce(finalDirection * finalForce, ForceMode.Impulse);
            
            Debug.Log($"공 타격! 파워: {currentPower * 100}% / 가해진 힘: {finalForce}");
        }
    }
}