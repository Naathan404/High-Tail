using UnityEngine;
using UnityEngine.EventSystems; // Bắt buộc phải có cho UI
using DG.Tweening; // Thư viện DOTween

public class TweenButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler,
    ISelectHandler, IDeselectHandler,
    ISubmitHandler
{
    [Header("Effect")]
    [SerializeField] private Vector3 _hoverScale = new Vector3(1.1f, 1.1f, 1.1f); // Khi chuột chỉ vào
    [SerializeField] private Vector3 _clickScale = new Vector3(0.9f, 0.9f, 0.9f); // Khi bấm lún xuống
    [SerializeField] private float _tweenDuration = 0.2f;
    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    private void OnDisable()
    {
        transform.DOKill();
        transform.localScale = _originalScale;
    }

    #region General effect
    private void Highlight()
    {
        transform.DOKill();
        transform.DOScale(_hoverScale, _tweenDuration)
                 .SetEase(Ease.OutBack)
                 .SetUpdate(true);
    }
    private void Unhighlight()
    {
        transform.DOKill();
        transform.DOScale(_originalScale, _tweenDuration)
                 .SetEase(Ease.OutQuad)
                 .SetUpdate(true);
    }
    #endregion

    #region Mouse Event
    public void OnPointerEnter(PointerEventData eventData) => Highlight();

    // 2. Chuột rời khỏi nút
    public void OnPointerExit(PointerEventData eventData) => Unhighlight();

    // 3. Chuột bấm click xuống
    public void OnPointerDown(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(_clickScale, _tweenDuration / 2f) // Tốc độ bấm xuống thường nhanh gấp đôi
                 .SetEase(Ease.OutQuad)
                 .SetUpdate(true);
    }

    // 4. Chuột nhả click ra
    public void OnPointerUp(PointerEventData eventData)
    {
        transform.DOKill();
        // Nhả ra thì nảy trở lại trạng thái Hover
        transform.DOScale(_hoverScale, _tweenDuration / 2f)
                 .SetEase(Ease.OutBack)
                 .SetUpdate(true);
    }
    #endregion


    #region Keyboard event
    public void OnSelect(BaseEventData eventData) => Highlight();

    public void OnDeselect(BaseEventData eventData) => Unhighlight();

    public void OnSubmit(BaseEventData eventData)
    {
        transform.DOKill();
        
        Sequence submitSequence = DOTween.Sequence();
        submitSequence.Append(transform.DOScale(_clickScale, _tweenDuration / 2f).SetEase(Ease.OutQuad))
                      .Append(transform.DOScale(_hoverScale, _tweenDuration / 2f).SetEase(Ease.OutBack))
                      .SetUpdate(true);
    }
    #endregion
}