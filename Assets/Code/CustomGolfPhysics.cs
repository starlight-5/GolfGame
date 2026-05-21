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

    private void OnCollisionStay(Collision collision)
    {
        // 충돌한 대상이 Terrain일 경우
        if (collision.gameObject.GetComponent<Terrain>() != null)
        {
            // 공의 현재 위치를 기반으로 텍스처 인덱스를 가져옴
            int textureIndex = GetMainTerrainTexture(transform.position, collision.gameObject.GetComponent<Terrain>());

            // 텍스처 인덱스에 따라 물리 계수 변경 (Layer 순서에 맞게 설정)
            if(textureIndex == 3) { // 그린 (4번째 텍스처)
                currentFriction = 1f;
                currentRestitution = 0f;
            }
            else if (textureIndex == 2) { // 벙커 (3번째 텍스처)
                currentFriction = 0.8f;
                currentRestitution = 0.2f;
            } else if (textureIndex == 1) { // 러프 (2번째 텍스처)
                currentFriction = 0.6f;
                currentRestitution = 0.4f;
            } else { // 페어웨이 (1번째 텍스처 - 기본값)
                currentFriction = 0.3f;
                currentRestitution = 0.6f;
            }
        }
    }

    // ---------------------------------------------------------
    // [핵심 기술] 현재 위치의 Terrain 텍스처 인덱스를 반환하는 함수
    // ---------------------------------------------------------
    private int GetMainTerrainTexture(Vector3 worldPos, Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        // 1. 월드 좌표를 Terrain 알파맵(Splatmap) 좌표로 변환
        int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
        int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

        // 2. 해당 좌표의 텍스처 혼합 비율(알파값) 배열을 가져옴
        float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        // 3. 가장 비율이 높은(가장 진하게 칠해진) 텍스처의 인덱스를 찾음
        int maxIndex = 0;
        float maxAlpha = 0f;

        for (int i = 0; i < splatmapData.GetLength(2); i++)
        {
            if (splatmapData[0, 0, i] > maxAlpha)
            {
                maxAlpha = splatmapData[0, 0, i];
                maxIndex = i;
            }
        }

        return maxIndex;
    }
    private void OnCollisionExit(Collision collision)
    {
        isInAir = true;
        isRolling = false;
    }
}