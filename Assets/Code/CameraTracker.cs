using UnityEngine;

public class CameraTracker : MonoBehaviour
{
    // 이전의 playerTransform 대신 골프공 컨트롤러를 직접 참조합니다.
    public GolfBallController ballController; 

    [Header("Camera Settings")]
    public Vector3 aimOffset = new Vector3(0, 3, -6); 
    public Vector3 trackOffset = new Vector3(0, 5, -8); 
    public float smoothSpeed = 10f; 

    public bool isTracking = false;

    void LateUpdate()
    {
        if (ballController == null) return;

        Vector3 targetPosition;
        Quaternion targetRotation;
        
        Transform ballTransform = ballController.transform;

        if (isTracking)
        {
            // [발사 후] 공을 따라다니는 트래킹
            targetPosition = ballTransform.position + trackOffset;
            targetRotation = Quaternion.LookRotation(ballTransform.position - transform.position);
        }
        else
        {
            // [발사 전] 컨트롤러의 조준 각도(aimYaw)를 읽어옵니다.
            Quaternion aimRotation = Quaternion.Euler(0, ballController.aimYaw, 0);
            
            // 공 위치를 기준으로 회전된 오프셋만큼 이동하여 등 뒤에 자리 잡음
            targetPosition = ballTransform.position + aimRotation * aimOffset;
            
            // 공이 날아갈 방향을 바라보되 시야를 살짝(10도) 내려다봄
            targetRotation = aimRotation * Quaternion.Euler(10f, 0, 0); 
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
    }
}