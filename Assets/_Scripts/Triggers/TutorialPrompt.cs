using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using TMPro;
using DG.Tweening;

public enum TutorialActionType { Move, Jump, Interact }

[RequireComponent(typeof(Collider2D))]
public class TutorialPrompt : MonoBehaviour
{
    [Header("Action Setup")]
    [SerializeField] private TutorialActionType _actionType;
    [SerializeField] private InputActionReference _actionRef;
    [SerializeField] private int _bindingIndex = 0;
    [SerializeField] private int _secondBindingIndex = -1; // optional: dùng cho Move (Left + Right cùng lúc), -1 = không dùng

    [Header("UI")]
    [SerializeField] private GameObject _promptRoot;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _instructionText;

    [Header("Localization")]
    [SerializeField] private LocalizedString _instructionLocalizedString;
    // Trong Localization Table, chuỗi dạng: "Nhấn {0} để di chuyển"
    // {0} sẽ được thay bằng tên phím hiện tại

    [Header("Timing")]
    [SerializeField] private float _bobAmplitude = 0.15f;
    [SerializeField] private float _bobDuration = 0.6f;
    [SerializeField] private float _fadeOutDuration = 1f;

    private bool _hasShown = false;
    private bool _learned = false;
    private Tween _bobTween;
    private float _originalLocalY;

    // Lấy live action từ InputManager (giống RebindButtonUI)
    private InputAction GetLiveAction()
    {
        if (_actionRef == null || InputManager.Instance == null || InputManager.Instance.Inputs == null)
            return null;
        return InputManager.Instance.Inputs.asset.FindAction(_actionRef.action.id);
    }

    private void Awake()
    {
        if (_promptRoot != null)
        {
            _originalLocalY = _promptRoot.transform.localPosition.y;
            _promptRoot.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_learned || _hasShown) return;
        if (!other.CompareTag("Player")) return;

        ShowPrompt();
    }

    private void ShowPrompt()
    {
        _hasShown = true;
        _canvasGroup.DOKill();
        _bobTween?.Kill();
        _canvasGroup.alpha = 1f;
        _promptRoot.SetActive(true);

        // Lấy tên phím hiện tại
        string keyName = InputDisplayUtils.GetBindingDisplayString(_actionRef, _bindingIndex);

        // Hiển thị dòng hướng dẫn với Smart String
        // 1 phím: "Nhấn {0} để nhảy"         → truyền (keyName)
        // 2 phím: "Nhấn {0} / {1} để di chuyển" → truyền (keyName, secondKeyName)
        if (_instructionText != null && _instructionLocalizedString != null)
        {
            if (_secondBindingIndex >= 0)
            {
                string secondKeyName = InputDisplayUtils.GetBindingDisplayString(_actionRef, _secondBindingIndex);
                _instructionText.text = _instructionLocalizedString.GetLocalizedString(keyName, secondKeyName);
            }
            else
            {
                _instructionText.text = _instructionLocalizedString.GetLocalizedString(keyName);
            }
        }

        if (_promptRoot != null)
            _promptRoot.SetActive(true);

        // Bob animation
        if (_promptRoot != null)
        {
            _bobTween = _promptRoot.transform
                .DOLocalMoveY(_originalLocalY + _bobAmplitude, _bobDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        var liveAction = GetLiveAction();
        if (liveAction != null)
            liveAction.performed += OnActionPerformed;
    }

    private void OnActionPerformed(InputAction.CallbackContext ctx)
    {
        if (_learned) return;
        _learned = true;

        var liveAction = GetLiveAction();
        if (liveAction != null)
            liveAction.performed -= OnActionPerformed;

        HidePrompt();
    }

    private void HidePrompt()
    {
        _bobTween?.Kill();
        if (_promptRoot != null)
        {
            var pos = _promptRoot.transform.localPosition;
            _promptRoot.transform.localPosition = new Vector3(pos.x, _originalLocalY, pos.z);

            _canvasGroup.DOFade(0f, _fadeOutDuration).OnComplete(() =>
            {
                _promptRoot.SetActive(false);
                _canvasGroup.alpha = 1f;
            });
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Nếu người chơi bước ra khỏi zone mà chưa học, ẩn prompt
        // để không bị trôi lơ lửng ngoài màn hình
        if (!other.CompareTag("Player") || _learned) return;
        if (!_hasShown) return;

        _hasShown = false;

        var liveAction = GetLiveAction();
        if (liveAction != null)
            liveAction.performed -= OnActionPerformed;

        HidePrompt();
    }

    private void OnDestroy()
    {
        _bobTween?.Kill();
        var liveAction = GetLiveAction();
        if (liveAction != null)
            liveAction.performed -= OnActionPerformed;
    }
}