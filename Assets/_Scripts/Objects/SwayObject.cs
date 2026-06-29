using DG.Tweening;
using UnityEngine;

public class SwayObject : MonoBehaviour
{
    [SerializeField] private float _angle = 30f;
    [SerializeField] private float _swayDuration = 1f;

    private void Start()
    {
        transform.DORotate(new Vector3(0, 0, this.transform.rotation.z + _angle), _swayDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }
}
