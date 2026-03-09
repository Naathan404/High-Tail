using DG.Tweening;
using UnityEngine;

public class BounceMushroom : MonoBehaviour, IBouncy
{
    [SerializeField] private Transform _visual;
    [SerializeField] private float _bouncyForce;
    public float BouncyForce => _bouncyForce;
    private Vector3 _originalScale;

    private void Start()
    {
        _originalScale = _visual.transform.localScale;
    }

    public void PlayBounceAnimation()
    {
        _visual.transform.DOKill();
        _visual.transform.DOScale(new Vector3(1.5f, 0.6f), 0.1f);
        _visual.transform.DOScale(_originalScale, 0.5f).SetEase(Ease.OutElastic);
    }
}
