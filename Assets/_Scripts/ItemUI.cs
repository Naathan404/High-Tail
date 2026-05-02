using UnityEngine;
using TMPro;
using DG.Tweening;

public class ItemUI : Singleton<ItemUI>
{
    [SerializeField] private TextMeshProUGUI _collectedItemText;
    [SerializeField] private RectTransform _iconTransform;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _showingTime = 2f;

    [HideInInspector] public Vector3 IconPosition;

    private void Update()
    {
        IconPosition = _iconTransform.position;
        Debug.DrawLine(transform.position, IconPosition, Color.red);
    }

    private void OnEnable()
    {
        ItemDataManager.OnItemCollected += UpdateUI;
    }

    private void OnDisable()
    {
        ItemDataManager.OnItemCollected -= UpdateUI;
    }

    private void UpdateUI()
    {
        _collectedItemText.text = ItemDataManager.Instance._itemData.CollectedItem.ToString();

        // Fade effect
        _canvasGroup.DOKill();

        _canvasGroup.DOFade(1f, 0.5f).
                    SetEase(Ease.OutQuad).
                    OnComplete(() =>
                    {
                        _canvasGroup.DOFade(0f, 0.5f).
                                SetDelay(_showingTime).
                                SetEase(Ease.InQuad);
                    });
    }
}