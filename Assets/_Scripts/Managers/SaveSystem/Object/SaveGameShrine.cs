using System;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class SaveGameShrine : MonoBehaviour, IInteractable
{
    [Header("Save settings")]
    private bool _canInteract = true;
    [SerializeField] private string _id;
    public string ID => _id;

    [Header("Detection")]
    [SerializeField] private LayerMask _playerLayer; // Layer của Player
    [SerializeField] private float _indicateRadius = 2f; // Bán kính kiểm tra player
    [SerializeField] private GameObject _visualIndicator;
    private bool _isPlayerNearby = false;

    [Header("Interact")]
    [SerializeField] private TextMeshPro saveInstruction;

    [Header("Floating Setting")]
    [SerializeField] private SpriteRenderer _visual;
    [SerializeField] private SpriteRenderer _interactionMark;
    [SerializeField] private float _floatingDuration = 1.5f;
    [SerializeField] private float _floatingYOffset = 1.5f;
    
    public static event Action<Transform> OnGameSaved;

    // 1. CHUYỂN START() THÀNH AWAKE()
    private void Awake()
    {
        EnableShrine(); // Mặc định mở lúc khởi tạo, SaveManager sẽ quyết định đóng hay mở sau
        _id = IDGenerator.GenerateUniqueID(gameObject);
        if (_visualIndicator != null) _visualIndicator.SetActive(false);
        if (saveInstruction != null)
        {
            saveInstruction.SetText("Press [E] to write your journey");
            saveInstruction.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        float originalY = _visual.transform.position.y;

        _visual.transform.DOMoveY(originalY + _floatingYOffset, _floatingDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
        
        _interactionMark.transform.DOMoveY(_interactionMark.transform.position.y + _floatingYOffset / 3f, _floatingDuration / 3f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        _interactionMark.gameObject.SetActive(false);
    }

    private void Update()
    {
        CheckForPlayer();
    }

//    #region Auto Generate ID
//#if UNITY_EDITOR
//    // 2. DÙNG ONVALIDATE ĐỂ CẬP NHẬT THEO THỜI GIAN THỰC KHI Ở TRONG EDITOR
//    private void OnValidate()
//    {
//        if (string.IsNullOrEmpty(_id))
//        {
//            GenerateUniqueID();
//        }
//        else
//        {
//            // Quét để kiểm tra xem có bị trùng ID do thao tác Duplicate (Ctrl + D) không
//            SaveGameShrine[] allShrines = FindObjectsByType<SaveGameShrine>(FindObjectsInactive.Include, FindObjectsSortMode.None);
//            foreach (var shrine in allShrines)
//            {
//                // Nếu tìm thấy một đền khác (không phải mình) mà ID y chang mình -> Bị trùng!
//                if (shrine != this && shrine.ID == this._id)
//                {
//                    Debug.Log($"[Auto-Fix] Đã phát hiện trùng ID tại {gameObject.name}. Đang tạo ID mới...");
//                    GenerateUniqueID();
//                    break;
//                }
//            }
//        }
//    }

//    // Gắn thêm nút này để bạn có thể Click chuột phải vào component và tự tạo lại ID nếu thích
//    [ContextMenu("Force Generate New ID")]
//    private void GenerateUniqueID()
//    {
//        _id = $"Shrine_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
//        UnityEditor.EditorUtility.SetDirty(this); // Đánh dấu để Unity lưu lại thay đổi
//    }
//#endif
//    #endregion

    #region IInteractable
    public bool CanInteract() => _canInteract;

    public void OnInteract(bool on = true)
    {
        if (!CanInteract()) return;
        SaveManager.Instance.activeShrine = on ? this : null;
        UIHelper.AnimateZoom(saveInstruction.gameObject, on);
        //UIHelper.AnimateZoom(_visualIndicator, !on);
    }

    public void Interact()
    {
        if (!CanInteract()) return;
        SaveManager.Instance.ExcuteSave();
        OnGameSaved?.Invoke(transform);
        CameraShakeManager.Instance.ShakeCustom(0.5f);
        GameManager.Instance.DoTimeFreeze(0.1f, 0.5f);
    }
    #endregion

    public void DisableShrine()
    {
        _canInteract = false;
        //GetComponent<SpriteRenderer>().color = Color.gray;
        UIHelper.AnimateZoom(saveInstruction.gameObject, false);
        ShowIndicator(false);
    }

    public void EnableShrine()
    {
        _canInteract = true;
        //GetComponent<SpriteRenderer>().color = Color.white;
    }

    #region Detection
    private void CheckForPlayer()
    {
        if (!_canInteract)
        {
            if (_isPlayerNearby)
            {
                _isPlayerNearby = false;
                ShowIndicator(false);
            }
            return;
        }
        // Kiểm tra xem có Player trong vùng bán kính không
        Collider2D player = Physics2D.OverlapCircle(transform.position, _indicateRadius, _playerLayer);

        if (player != null)
        {
            if (!_isPlayerNearby)
            {
                _isPlayerNearby = true;
                ShowIndicator(true);
            }
        }
        else
        {
            if (_isPlayerNearby)
            {
                _isPlayerNearby = false;
                ShowIndicator(false);
            }
        }
    }

    private void ShowIndicator(bool show)
    {
        // Chỉ hiện bảng khi Shrine đang hoạt động và có player ở gần
        // if (_visualIndicator != null)
        // {
        //     UIHelper.AnimateZoom(_visualIndicator, show && _canInteract);
        // }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_canInteract)
            _interactionMark.gameObject.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _interactionMark.gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _indicateRadius);
    }
    #endregion
}