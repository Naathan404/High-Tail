using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BiomeNotifier : Singleton<BiomeNotifier>
{
    [Header("UI Settings")]
    //[SerializeField] private string biomeDescription;
    [SerializeField] private float _displayDuration = 1.5f;
    [SerializeField] private float _fadeInDuration = 2f;
    [SerializeField] private float _fadeOutDuration = 2f;
    private TextMeshProUGUI _text;

    public override void Awake()
    {
        base.Awake();
        _text = GetComponentInChildren<TextMeshProUGUI>();

        _text.DOFade(0f, 0f);
    }

    public void ShowBiomeNotification(string biomeName)
    {
        StartCoroutine(ShowNotifyRoutine(biomeName));
    }

    private IEnumerator ShowNotifyRoutine(string biomeName)
    {
        _text.text = biomeName;
        _text.DOFade(1f, _fadeInDuration);
        yield return new WaitForSeconds(_displayDuration + _fadeInDuration);
        _text.DOFade(0f, _fadeOutDuration);    
    }
}
