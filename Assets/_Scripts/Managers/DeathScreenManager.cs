using System.Collections;
using DG.Tweening;
using UnityEngine;

public class DeathScreenManager : Singleton<DeathScreenManager>
{
    [SerializeField] private Material _irisMaterial;
    [SerializeField] private float _closeDuration = 0.2f; 
    [SerializeField] private float _openDuration = 0.35f;  
    [SerializeField] private float _blackoutTime = 0.2f;  

    public static event System.Action OnDeathScreenTriggered;

    private void Start()
    {
        _irisMaterial.SetFloat("_IrisWipeRadius", 1.5f); 
        _irisMaterial.SetVector("_IrisWipeCenter", new Vector2(0.5f, 0.5f));
    }

    public void TriggerDeathWipe(Vector3 deathPosition, Vector3 respawnPosition, System.Action onRespawnReady, float respawnDuration)
    {
        StartCoroutine(DeathRoutine(deathPosition, respawnPosition, onRespawnReady, respawnDuration));
    }

    private IEnumerator DeathRoutine(Vector3 deathPos, Vector3 respawnPos, System.Action onRespawnReady, float respawnDuration)
    {
        Vector2 screenDeathPos = Camera.main.WorldToViewportPoint(deathPos);
        _irisMaterial.SetVector("_IrisWipeCenter", screenDeathPos);

        _irisMaterial.DOFloat(0f, "_IrisWipeRadius", respawnDuration).SetEase(Ease.InCubic);
        
        yield return new WaitForSeconds(respawnDuration);
        onRespawnReady?.Invoke(); 
        
        // đợi màn hình đen
        OnDeathScreenTriggered?.Invoke();
        yield return new WaitForSeconds(_blackoutTime);

        // mở lỗ tròn ở vị trí hồi sinh
        Vector2 screenRespawnPos = Camera.main.WorldToViewportPoint(respawnPos);
        _irisMaterial.SetVector("_IrisWipeCenter", screenRespawnPos);
        
        // phóng rộng lỗ đen ra
        _irisMaterial.DOFloat(2f, "_IrisWipeRadius", _openDuration).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(_openDuration);
        _irisMaterial.SetVector("_IrisWipeCenter", new Vector2(0.5f, 0.5f));
    }    
}
