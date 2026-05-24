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
       if (currentVelocity.magnitude > 0.05f) 
        {
            if (isInAir)
            {
                currentVelocity.y -= gravity * Time.fixedDeltaTime;
                Vector3 windAcceleration = windForce / rb.mass;
                currentVelocity += windAcceleration * Time.fixedDeltaTime;
            }
            
            if (isRolling)
            {
                Vector3 frictionForce = -currentVelocity.normalized * currentFriction * gravity * Time.fixedDeltaTime;
                if (frictionForce.magnitude >= currentVelocity.magnitude)
                    currentVelocity = Vector3.zero;
                else
                    currentVelocity += frictionForce;
            }

            rb.linearVelocity = currentVelocity; 
        }
        else
        {
            // ★ [핵심 추가] 속도가 미세해지면 강제로 완벽한 정지 상태로 만듦 ★
            currentVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            isInAir = false;
            isRolling = false;
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
        // 1. 강제로 구르기 상태 고정 (미세 바운스로 인해 공중으로 판정되는 것 방지)
        isInAir = false;
        isRolling = true;

        // 2. 충돌한 바닥이 Terrain인지 확인
        Terrain terrain = collision.gameObject.GetComponent<Terrain>();
        
        if (terrain != null)
        {
            // 현재 공의 월드 좌표를 넘겨주어 가장 진하게 칠해진 텍스처 번호를 알아냄
            int textureIndex = GetMainTerrainTexture(transform.position, terrain);

            // Terrain Layer에 등록한 순서(0번부터 시작)에 따라 물리 계수 적용
            if (textureIndex == 2) { 
                // 3번째 텍스처 (벙커)
                currentFriction = 0.8f;
                currentRestitution = 0.1f;
            } 
            else if (textureIndex == 1) { 
                // 2번째 텍스처 (러프)
                currentFriction = 0.6f;
                currentRestitution = 0.3f;
            } 
            else { 
                // 1번째 텍스처 (페어웨이 - 기본값)
                currentFriction = 0.3f;
                currentRestitution = 0.6f;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // 바닥에서 완전히 튕겨져 나갔을 때만 공중 상태로 전환
        isInAir = true;
        isRolling = false;
    }

    // ---------------------------------------------------------
    // [핵심 기술] 현재 위치의 Terrain 텍스처 인덱스를 반환하는 함수
    // ---------------------------------------------------------
    private int GetMainTerrainTexture(Vector3 worldPos, Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        // 1. 월드 좌표를 Terrain 알파맵(Splatmap) 내부의 2D 격자 좌표로 변환
        int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
        int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

        // 좌표가 지형 경계를 벗어날 경우를 대비한 안전 장치 (에러 방지)
        mapX = Mathf.Clamp(mapX, 0, terrainData.alphamapWidth - 1);
        mapZ = Mathf.Clamp(mapZ, 0, terrainData.alphamapHeight - 1);

        // 2. 해당 좌표의 텍스처 혼합 비율(알파값) 배열을 가져옴
        float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        // 3. 배열을 순회하며 가장 비율이 높은(가장 진하게 칠해진) 텍스처의 인덱스를 찾음
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

        return maxIndex; // 0, 1, 2 중 하나를 반환
    }

}