using Unity.Cinemachine; // Dùng namespace mới
using UnityEngine;

public class CameraShakeManager : Singleton<CameraShakeManager>
{
    [Header("Impulse Sources")]
    public CinemachineImpulseSource dashShakeSource;
    public CinemachineImpulseSource damageShakeSource;
    public CinemachineImpulseSource landingShakeSource;
    public CinemachineImpulseSource defaultShakeSource;


    public void ShakeForDash()
    {
        if (dashShakeSource != null) dashShakeSource.GenerateImpulse();
    }

    public void ShakeForDamage()
    {
        if (damageShakeSource != null) damageShakeSource.GenerateImpulse();
    }

    public void ShakeForLanding()
    {
        if(landingShakeSource != null) landingShakeSource.GenerateImpulse();
    }

    public void ShakeCustom(float forceMultiplier)
    {
        if (defaultShakeSource != null) defaultShakeSource.GenerateImpulseWithForce(forceMultiplier);
    }
}