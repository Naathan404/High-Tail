using System.Collections;
using UnityEngine;
using DG.Tweening;
using System;

public class AstralPulseSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    public float pulseRadius = 10f; 
    public float pulseSpeed = 20f;  
    public float cooldownTime = 3f;
    private bool isCooldown = false;
    
    public LayerMask interactableLayer;

    [Header("Zero-Asset VFX")]
    [Tooltip("Kéo GameObject chứa LineRenderer vào đây")]
    public LineRenderer ringRenderer;
    [ColorUsage(true, true)] 
    public Color pulseColor = Color.cyan;
    public float ringThickness = 0.2f;

    public static event Action<Transform> OnPulsed;

    private void Awake()
    {
        if (ringRenderer != null)
        {
            SetupCircle();
        }
    }

    private void SetupCircle()
    {
        int segments = 100; 
        ringRenderer.positionCount = segments + 1;
        ringRenderer.useWorldSpace = false;
        ringRenderer.startWidth = ringThickness;
        ringRenderer.endWidth = ringThickness;
        
        // Công thức vẽ đường tròn bán kính 1
        float angle = 0f;
        for (int i = 0; i < (segments + 1); i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle);
            float y = Mathf.Cos(Mathf.Deg2Rad * angle);
            ringRenderer.SetPosition(i, new Vector3(x, y, 0));
            angle += (360f / segments);
        }
        
        ringRenderer.gameObject.SetActive(false); 
    }

    public void CastPulse()
    {
        if (isCooldown) return;
        StartCoroutine(CooldownRoutine());

        PlayLineVFX();

        // Logic tìm và kích hoạt nấm (Giữ nguyên)
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, pulseRadius, interactableLayer);
        foreach (Collider2D hit in hitObjects)
        {
            ILightPulseReactive reactiveObj = hit.GetComponent<ILightPulseReactive>();
            if (reactiveObj != null)
            {
                float delayTime = Vector2.Distance(transform.position, hit.transform.position) / pulseSpeed;
                StartCoroutine(TriggerReactionWithDelay(reactiveObj, delayTime));
            }
        }
    }

    private void PlayLineVFX()
    {
        if (ringRenderer == null) return;

        ringRenderer.gameObject.SetActive(true);
        ringRenderer.transform.localScale = Vector3.zero;

        float duration = pulseRadius / pulseSpeed;

        OnPulsed?.Invoke(transform);
        GameManager.Instance.DoTimeFreeze(0.05f, 0.1f);
        CameraShakeManager.Instance.ShakeCustom(0.1f);
        
        Sequence vfxSeq = DOTween.Sequence();

        
        vfxSeq.Join(ringRenderer.transform.DOScale(Vector3.one * pulseRadius, duration)
            .SetEase(Ease.OutExpo));

        vfxSeq.Join(DOVirtual.Float(ringThickness, 0f, duration, w => 
        {
            ringRenderer.startWidth = w;
            ringRenderer.endWidth = w;
        }).SetEase(Ease.OutQuad));

        vfxSeq.Join(DOVirtual.Float(0f, 1f, duration, t => 
        {
            Color currentColor;
            if (t < 0.15f) 
            {
                currentColor = Color.Lerp(Color.white, pulseColor, t / 0.15f);
            } 
            else 
            {
                Color transparentColor = new Color(pulseColor.r, pulseColor.g, pulseColor.b, 0f);
                currentColor = Color.Lerp(pulseColor, transparentColor, (t - 0.15f) / 0.85f);
            }
            ringRenderer.startColor = currentColor;
            ringRenderer.endColor = currentColor;
            
        }).SetEase(Ease.Linear));

        // Xong thì tắt đi
        vfxSeq.OnComplete(() => 
        {
            ringRenderer.gameObject.SetActive(false);
        });
    }

    private IEnumerator TriggerReactionWithDelay(ILightPulseReactive obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.ReactToLightPulse(); 
    }

    private IEnumerator CooldownRoutine()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isCooldown = false;
    }
}