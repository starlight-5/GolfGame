using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomGolfPhysics : MonoBehaviour
{
    private Rigidbody rb;
    
    [Header("Environment (환경 및 외력)")]
    public Vector3 windForce = new Vector3(2f, 0f, 1f); // 외력 - 바람
    public float gravity = 9.81f; // 중력 가속도
    
    [Header("Physics State (현재 상태)")]
    public float currentFriction = 0.3f; // 마찰계수
    public float currentRestitution = 0.6f; // 반발계수 (Bounciness)

    public bool isInAir = false;
    private bool isRolling = false;
    public Vector3 currentVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // 기본 중력 비활성화 (커스텀 제어)
    }

    void FixedUpdate()
    {
        if (currentVelocity.magnitude > 0.01f)
        {
            if (isInAir)
            {
                // 1. 발사체 운동 (중력 적용: v = v0 - gt)
                currentVelocity.y -= gravity * Time.fixedDeltaTime;
                
                // 2. 외력 - 바람 적용 (가속도 = 힘/질량, v = v0 + at)
                Vector3 windAcceleration = windForce / rb.mass;
                currentVelocity += windAcceleration * Time.fixedDeltaTime;
            }
            
            if (isRolling)
            {
                // 3. 표면별 마찰 적용 (감속)
                Vector3 frictionForce = -currentVelocity.normalized * currentFriction * gravity * Time.fixedDeltaTime;
                if (frictionForce.magnitude >= currentVelocity.magnitude)
                    currentVelocity = Vector3.zero;
                else
                    currentVelocity += frictionForce;
            }

            // Rigidbody 속도에 직접 계산한 속도 대입
            rb.linearVelocity = currentVelocity; 
        }
    }

    public void HitBall(Vector3 force)
    {
        currentVelocity = force / rb.mass; // a = F/m
        isInAir = true;
        isRolling = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 4. 충돌 & 반발 수동 구현
        Vector3 normal = collision.contacts[0].normal;
        float vDotN = Vector3.Dot(currentVelocity, normal);
        
        // 물체가 표면을 향해 이동 중일 때만 반사 계산
        if (vDotN < 0)
        {
            // 반발계수(e)를 적용한 반사 벡터 공식: v_out = v_in - (1 + e) * (v_in · n) * n
            currentVelocity = currentVelocity - (1f + currentRestitution) * vDotN * normal;
            
            // 바운스가 충분히 작아지면 구르기 상태로 전환
            if (Mathf.Abs(currentVelocity.y) < 0.5f) 
            {
                isInAir = false;
                isRolling = true;
                currentVelocity.y = 0;
            }
        }
        
        // 표면에 따른 마찰/반발계수 변화
        if (collision.gameObject.CompareTag("Bunker")) {
            currentFriction = 0.8f;
            currentRestitution = 0.2f;
        } else if (collision.gameObject.CompareTag("Rough")) {
            currentFriction = 0.6f;
            currentRestitution = 0.4f;
        } else { // Fairway
            currentFriction = 0.3f;
            currentRestitution = 0.6f;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isInAir = true;
        isRolling = false;
    }
}