using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallPhysics : MonoBehaviour
{
    private Rigidbody rb;
    public float forceMultiplier = 0.5f; // 파워 조절용 수치
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // PlayerController에서 타격할 때 이 함수를 호출합니다!
    public void ApplyHit(HitData data)
    {
        // 1. 발사 각도 계산
        Vector3 launchDirection = Quaternion.AngleAxis(-data.LaunchAngle, transform.right) * data.Direction;
        
        // 2. 임팩트 정확도(팡야 타이밍)에 따른 좌우 휘어짐 보정
        launchDirection = Quaternion.AngleAxis(data.Accuracy, Vector3.up) * launchDirection;

        // 3. 최종 파워 계산
        float finalForce = data.Power * forceMultiplier;

        // 4. 물리 엔진(Rigidbody)에 쳐올리는 힘(Impulse) 가하기
        rb.isKinematic = false;
        rb.AddForce(launchDirection.normalized * finalForce, ForceMode.Impulse);
        
        Debug.Log($"공 발사됨! 가해진 힘: {finalForce}");
    }
}