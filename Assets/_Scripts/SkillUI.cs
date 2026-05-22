using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkillUI : Singleton<SkillUI>
{
    [SerializeField] private Image _skillIcon;
    [SerializeField] private float _iconDuration = 2f;
    [HideInInspector] public float skillCoolDown;
    private float _coolDownTimer;

    public override void Awake()
    {
        base.Awake();
        _coolDownTimer = skillCoolDown;
        _skillIcon.DOFade(0f, 0f);
    }

    private void Update()
    {
        _coolDownTimer -= Time.deltaTime;
    }

    public void UpdateUI()
    {
        if (_coolDownTimer > 0)
        {
            _skillIcon.fillAmount = _coolDownTimer / skillCoolDown;
        }
        else
        {
            _skillIcon.fillAmount = 1;
        }
    }

    public void ShowIcon()
    {
        StartCoroutine(Show());
    }

    private IEnumerator Show()
    {
        _skillIcon.DOFade(1f, 2f);
        yield return new WaitForSeconds(2f);
        _skillIcon.DOFade(0f, 2f);
    }
}
