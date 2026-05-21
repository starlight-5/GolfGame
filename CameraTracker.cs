using UnityEngine;

public class CameraTracker : MonoBehaviour
{
    public Transform ballTransform;
    public Transform playerTransform; // 공의 위치와 회전을 기준점으로 잡을 오브젝트

    [Header("Camera Settings")]
    public Vector3 aimOffset = new Vector3(0, 2, -4);
    public Vector3 trackOffset = new Vector3(0, 3, -6);
    public float smoothSpeed = 5f;

    public bool isTracking = false;

    void LateUpdate()
    {
        Vector3 targetPosition;
        Quaternion targetRotation;

        if (isTracking)
        {
            // 6. 트래킹 카메라 (Follow Camera)
            targetPosition = ballTransform.position + trackOffset;
            targetRotation = Quaternion.LookRotation(ballTransform.position - transform.position);
        }
        else
        {
            // 6. 조준 카메라 (수평 회전 설정 반영)
            targetPosition = playerTransform.position + playerTransform.TransformDirection(aimOffset);
            targetRotation = Quaternion.LookRotation(playerTransform.position - transform.position);
        }

        // Lerp와 Slerp를 이용해 부드러운 시점 전환
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
    }
}