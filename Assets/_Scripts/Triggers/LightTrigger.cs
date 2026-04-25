using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightTrigger : MonoBehaviour
{
    [SerializeField] private List<LightData> _lightsToToggle;
    [SerializeField] private bool _isActivated = false;

    private void Start()
    {
        foreach (var light in _lightsToToggle)
        {
            light.LightToToggle.intensity = 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (_isActivated) return;
            _isActivated = true;
            ToggleLights();
        }
    }

    private void ToggleLights()
    {
        foreach (var light in _lightsToToggle)
        {
            light.LightToToggle.intensity = 0f;
            StartCoroutine(ToggleLightCoroutine(light));
            
        }
    }

    private IEnumerator ToggleLightCoroutine(LightData light)
    {
        yield return new WaitForSeconds(light.TimeToToggle);
        
        DOTween.To(() => light.LightToToggle.intensity, x => light.LightToToggle.intensity = x, light.MaxIntensity, light.FadeDuration).SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(light.FadeDuration);

        DOTween.To(() => light.LightToToggle.intensity, x => light.LightToToggle.intensity = x, light.MinIntensity, light.LoopDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

        if (light.IsOneTimeTrigger)
        {
            _lightsToToggle.Remove(light);
        }
    }

    [Serializable]
    public struct LightData
    {
        public Light2D LightToToggle;
        public float FadeDuration;
        public float LoopDuration;
        public float TimeToToggle;
        public bool IsOneTimeTrigger;

        public float MinIntensity;
        public float MaxIntensity;
    }
}
