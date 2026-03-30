using DG.Tweening;
using System.Collections;
using UnityEngine;

public class FloatingPlatform : MonoBehaviour
{
    [SerializeField] private float _interval; // The time when platform start to move until it return to original position
    [SerializeField] private float _depth;

    private Trigger _trigger;
    private Vector3 _originalPosition;
    private Vector3 _targetPosition;
    private bool _isSteppedOn;
    private float _force;

    private void Awake()
    {
        _trigger = GetComponentInChildren<Trigger>();
        _originalPosition = transform.position;
        _targetPosition = transform.position + Vector3.down * _depth;
    }

    private void OnEnable()
    {
        _trigger.OnStepIn += Bound;
    }

    private void OnDisable()
    {
        _trigger.OnStepIn -= Bound;
    }

    private void Bound()
    {
        if (_isSteppedOn) return;
        _isSteppedOn = true;

        // Tạo một Sequence (chuỗi hành động)
        Sequence bounceSequence = DOTween.Sequence();

        // 1. Đi xuống (Lún) - Dùng OutQuad để nó lún xuống nhanh và êm
        bounceSequence.Append(transform.DOMove(_targetPosition, _interval / 2).SetEase(Ease.OutQuad));

        // 2. Nảy lên lại vị trí cũ - Dùng OutElastic hoặc OutBack để có độ "nhún"
        bounceSequence.Append(transform.DOMove(_originalPosition, _interval / 2).SetEase(Ease.OutElastic));

        // 3. Khi xong hết thì cho phép đạp tiếp
        bounceSequence.OnComplete(() => _isSteppedOn = false);
    }

    private IEnumerator Rebound()
    {
        transform.DOMove(_originalPosition, _interval / 2).SetEase(Ease.OutBounce);
        yield return new WaitForSeconds(_interval / 2);
        _isSteppedOn = false;
    }
}
