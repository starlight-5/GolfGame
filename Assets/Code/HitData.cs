using UnityEngine;

public struct HitData
{
    public float Power;          
    public float Accuracy;       
    public Vector3 Direction;    
    public float LaunchAngle;    
    public float Spin;           

    public HitData(float power, float accuracy, Vector3 direction, float launchAngle, float spin)
    {
        Power = power;
        Accuracy = accuracy;
        Direction = direction.normalized;
        LaunchAngle = launchAngle;
        Spin = spin;
    }
}