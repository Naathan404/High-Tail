using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

public enum TutorialActionType { Move, Jump, Interact }

[RequireComponent(typeof(Collider2D))]
public class TutorialPrompt : MonoBehaviour
{
    [Header("Action Setup")]
    [SerializeField] private TutorialActionType _actionType;
    [SerializeField] private InputActionReference _actionRef;
    [SerializeField] private int _bindingIndex = 0; // nếu Move (Vector2 composite), set đúng part binding cần dạy (vd Left hoặc Right)

    [Header("UI")]
    [SerializeField] private GameObject _promptRoot;       // object cha chứa icon + text, đặt World Space Canvas phía trên nhân vật/zone
    // [SerializeField] private CanvasGroup _promptCanvasGroup;
    [SerializeField] private TextMeshProUGUI _keyText;
    //[SerializeField] private TextMeshProUGUI _actionLabelText; // optional: "Di chuyển", "Nhảy", "Tương tác"

    [Header("Timing")]
    [SerializeField] private float _fadeInDuration = 0.25f;
    [SerializeField] private float _fadeOutDuration = 0.3f;
    [SerializeField] private float _bobAmplitude = 0.15f;
    [SerializeField] private float _bobDuration = 0.6f;

    // [Header("Once Per Game")]
    // [SerializeField] private bool _persistAcrossPlaythrough = false; // nếu true, dùng PlayerPrefs để không hiện lại nếu đã học rồi (kể cả load lại scene)
    // [SerializeField] private string _saveKey = "tutorial_move";       // key riêng cho từng prompt nếu persist

    private bool _hasShown = false;
    private bool _learned = false;
    private Tween _bobTween;

    private void Awake()
    {
        if (_promptRoot != null)
            _promptRoot.SetActive(false);

        // if (_promptCanvasGroup != null)
        //     _promptCanvasGroup.alpha = 0f;

        // if (_persistAcrossPlaythrough && PlayerPrefs.GetInt(_saveKey, 0) == 1)
        // {
        //     _learned = true;
        // }
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

        if (_keyText != null)
        {
            string keyName = InputDisplayUtils.GetBindingDisplayString(_actionRef, _bindingIndex);
            _keyText.text = keyName;
        }

        if (_promptRoot != null)
            _promptRoot.SetActive(true);

        // if (_promptCanvasGroup != null)
        //     _promptCanvasGroup.DOFade(1f, _fadeInDuration);

        // Hiệu ứng nhấp nhô nhẹ để thu hút mắt
        if (_promptRoot != null)
        {
            _bobTween = _promptRoot.transform
                .DOLocalMoveY(_promptRoot.transform.localPosition.y + _bobAmplitude, _bobDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        if (_actionRef != null && _actionRef.action != null)
            _actionRef.action.performed += OnActionPerformed;
    }

    private void OnActionPerformed(InputAction.CallbackContext ctx)
    {
        if (_learned) return;
        _learned = true;

        if (_actionRef != null && _actionRef.action != null)
            _actionRef.action.performed -= OnActionPerformed;

        // if (_persistAcrossPlaythrough)
        //     PlayerPrefs.SetInt(_saveKey, 1);

        HidePrompt();
    }

    private void HidePrompt()
    {
        _bobTween?.Kill();

        // if (_promptCanvasGroup != null)
        // {
        //     _promptCanvasGroup.DOFade(0f, _fadeOutDuration)
        //         .OnComplete(() =>
        //         {
        //             if (_promptRoot != null)
        //                 _promptRoot.SetActive(false);
        //         });
        // }
        if (_promptRoot != null)
        {
            _promptRoot.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        _bobTween?.Kill();
        if (_actionRef != null && _actionRef.action != null)
            _actionRef.action.performed -= OnActionPerformed;
    }
}