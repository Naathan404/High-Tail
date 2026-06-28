using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkillUI : Singleton<SkillUI>
{
    [SerializeField] private Image _skillIcon;
    [SerializeField] private float _iconDuration = 2f;
    [SerializeField] private float _heightOffset = 1.5f;
    [SerializeField] private Transform _player;
    [HideInInspector] public float skillDuration;
    private float _skillTimer;
    private bool _isIconVisible;

    public override void Awake()
    {
        base.Awake();
        if (_player == null)
        {
            Debug.LogError("Player transform is not assigned in SkillUI.");
        }

        _skillTimer = skillDuration;
        _skillIcon.DOFade(0f, 0f);
    }

    private void Update()
    {
        if (_isIconVisible)
        {
            _skillTimer -= Time.deltaTime;
            _skillIcon.fillAmount = Mathf.Clamp01(_skillTimer / skillDuration);
        }
    }

    private void LateUpdate()
    {
        transform.position = _player.position + Vector3.up * _heightOffset;
    }

    public void ShowIcon()
    {
        if (_isIconVisible) return;
        StartCoroutine(Show());
    }

    private IEnumerator Show()
    {
        _skillTimer = skillDuration;
        _isIconVisible = true;
        _skillIcon.DOFade(1f, 0f);
        _skillIcon.DOFade(0.5f, skillDuration);

        yield return new WaitForSeconds(skillDuration);

        // skill cooldown
        _isIconVisible = false;
    }
}
