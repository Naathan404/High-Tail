using UnityEngine;
using DG.Tweening;

// Khi nguoi choi nhay len platform , platform se di chuyen den target position, sau do tu dong quay lai vi tri ban dau
// Neu nguoi choi roi khoi platform thi platform se quay lai vi tri ban dau ngay lap tuc

public class OnMovingPlatform : MonoBehaviour
{
    [SerializeField] private float _movingTime = 0.5f;
    [SerializeField] private float _returnTime = 0.5f;

    private Vector3 _originalPosition;
    private Vector3 _targetPosition;
    private Trigger _trigger;

    private void Awake()
    {
        _originalPosition = transform.position;
        Transform targetTransform = transform.Find("TargetPosition");
        if (targetTransform != null)
            _targetPosition = targetTransform.position;
        _trigger = GetComponentInChildren<Trigger>();
    }

    private void OnEnable()
    {
        _trigger.OnStepIn += MoveToTarget;
    }

    private void OnDisable()
    {
        _trigger.OnStepIn -= MoveToTarget;
    }

    private void MoveToTarget()
    {
        this.transform.DOMove(_targetPosition, _movingTime)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => ReturnToOriginalPosition());
    }

    private void ReturnToOriginalPosition()
    {
        this.transform.DOMove(_originalPosition, _returnTime)
                .SetEase(Ease.OutQuart);
    }

    #region SetParents 

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            collision.collider.transform.SetParent(this.transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            collision.collider.transform.SetParent(null);
            // Return the platform to its original position immediately when the player steps off
            ReturnToOriginalPosition();
        }
    }
    #endregion
}
