using UnityEngine;
using Unity.Cinemachine;

public class CamManager : MonoBehaviour
{
    [Header("Cinemachine Virtual Cameras")]
    public CinemachineCamera aimingCamera;
    public CinemachineCamera impactCamera;
    public CinemachineCamera flightCamera;

    [Header("Targets")]
    public Transform playerTransform;
    public Transform ballTransform;
    void Start()
    {
        // 초기 셋팅: 조준 카메라 활성화
        SetAimingView();
        
        // 카메라가 추적할 대상 할당
        aimingCamera.LookAt = playerTransform;
        impactCamera.LookAt = ballTransform;
        
        // 비행 카메라는 공을 따라다니며(Follow) 바라보도록(LookAt) 설정
        flightCamera.Follow = ballTransform;
        flightCamera.LookAt = ballTransform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetAimingView()
    {
        aimingCamera.Priority = 10;
        impactCamera.Priority = 0;
        flightCamera.Priority = 0;
    }

    // 2. 임팩트 (팡야 스윙 순간) - 조리개 개방 및 줌인 연출용 카메라
    public void SetImpactView()
    {
        aimingCamera.Priority = 0;
        impactCamera.Priority = 10;
        flightCamera.Priority = 0;
    }

    // 3. 공 비행 추적
    public void SetFlightTrackingView()
    {
        aimingCamera.Priority = 0;
        impactCamera.Priority = 0;
        flightCamera.Priority = 10;
    }
}
