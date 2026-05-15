using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [Header("Skill Unlock Setting")]
    [SerializeField] private GameObject _skillUnlockedPanel;
    [SerializeField] private TextMeshProUGUI _skillUnlockedText;
    [SerializeField] private TextMeshProUGUI _howToUseText;
    [SerializeField] private CanvasGroup _panelCanvasGroup; 
    

    [Header("Icons")]
    [SerializeField] private Image _checkpointIcon;
    [SerializeField] private float _fadeDuration = 0.5f;
    

    [Header("Animation Settings")]
    [SerializeField] private float _animationDuration = 0.5f;   // thời gian anim ẩn/hiện
    [SerializeField] private float _typewriterDuration = 1.0f; // thời gian đánh text
    [SerializeField] private float _skillUnlockedPanelShowTime = 3f;    // thời gian tồn tại 
    [SerializeField] private float _howToUseFadeDuration = 0.5f;

    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI _deathCountText;
    private int _deathCount = 0;

    private void Start()
    {
        if (_skillUnlockedPanel != null)
        {
            _skillUnlockedPanel.SetActive(false);
        }

        _deathCountText.text = _deathCount.ToString();
        _checkpointIcon.gameObject.SetActive(false);
    }

    public void ShowSkillUnlocked(LocalizedString skillName, LocalizedString skillDes, params object[] desArgs)
    {
        StartCoroutine(SkillUnlockRoutine(skillName, skillDes, desArgs));
    }

    private IEnumerator SkillUnlockRoutine(LocalizedString skillName, LocalizedString skillDes, object[] desArgs)
    {
        _skillUnlockedPanel.SetActive(true);
        _howToUseText.text = "";
        _howToUseText.alpha = 0f;
        _skillUnlockedText.text = ""; 
        
        RectTransform panelRect = _skillUnlockedPanel.GetComponent<RectTransform>();
        panelRect.localScale = Vector3.zero; 
        
        if (_panelCanvasGroup != null) 
            _panelCanvasGroup.alpha = 0f;

        // Lấy chuỗi đã được dịch
        string localizedName = skillName.GetLocalizedString();
        string localizedDes = "";
        if (desArgs != null && desArgs.Length > 0)
        {
            localizedDes = skillDes.GetLocalizedString(desArgs);
        }
        else
        {
            localizedDes = skillDes.GetLocalizedString();
        }

        // anim hiện lên
        panelRect.DOScale(Vector3.one, _animationDuration).SetEase(Ease.OutBack).SetUpdate(true);
        if (_panelCanvasGroup != null)
            _panelCanvasGroup.DOFade(1f, _animationDuration).SetUpdate(true);
        yield return new WaitForSecondsRealtime(_animationDuration);

        // anim gõ ghữ
        _skillUnlockedText.text = localizedName;
        _skillUnlockedText.maxVisibleCharacters = 0;
        DOTween.To(() => _skillUnlockedText.maxVisibleCharacters, 
                x => _skillUnlockedText.maxVisibleCharacters = x, 
                localizedName.Length, 
                _typewriterDuration).SetUpdate(true);

        _howToUseText.text = localizedDes;
        _howToUseText.DOFade(1f, _howToUseFadeDuration);
        yield return new WaitForSecondsRealtime(_typewriterDuration);

        // đợi hết thời gian tồn tại
        yield return new WaitForSecondsRealtime(_skillUnlockedPanelShowTime);

        // anim ẩn đi
        panelRect.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).SetUpdate(true);
        if (_panelCanvasGroup != null)
            _panelCanvasGroup.DOFade(0f, 0.5f).SetUpdate(true);

        yield return new WaitForSecondsRealtime(0.5f);
        _skillUnlockedPanel.SetActive(false);
    }

    public void AddDeathCount()
    {
        _deathCount++;
        _deathCountText.text = _deathCount.ToString();
    }

    public void PlayCheckpointIcon()
    {
        _checkpointIcon.DOKill();
        _checkpointIcon.gameObject.SetActive(true);
        _checkpointIcon.DOFade(1f, _fadeDuration).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            _checkpointIcon.DOFade(0f, _fadeDuration).SetEase(Ease.InOutCubic).OnComplete(() =>
            {
                _checkpointIcon.gameObject.SetActive(false);
            });
        });
    }
}