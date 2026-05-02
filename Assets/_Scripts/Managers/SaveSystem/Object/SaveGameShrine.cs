using System;
using TMPro;
using UnityEngine;

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

    // 1. CHUYỂN START() THÀNH AWAKE()
    private void Awake()
    {
        EnableShrine(); // Mặc định mở lúc khởi tạo, SaveManager sẽ quyết định đóng hay mở sau

        if (_visualIndicator != null) _visualIndicator.SetActive(false);
        if (saveInstruction != null)
        {
            saveInstruction.SetText("Press up arrow to save");
            saveInstruction.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        CheckForPlayer();
    }

    #region Auto Generate ID (Chống trùng lặp)
#if UNITY_EDITOR
    // 2. DÙNG ONVALIDATE ĐỂ CẬP NHẬT THEO THỜI GIAN THỰC KHI Ở TRONG EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_id))
        {
            GenerateUniqueID();
        }
        else
        {
            // Quét để kiểm tra xem có bị trùng ID do thao tác Duplicate (Ctrl + D) không
            SaveGameShrine[] allShrines = FindObjectsByType<SaveGameShrine>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var shrine in allShrines)
            {
                // Nếu tìm thấy một đền khác (không phải mình) mà ID y chang mình -> Bị trùng!
                if (shrine != this && shrine.ID == this._id)
                {
                    Debug.Log($"[Auto-Fix] Đã phát hiện trùng ID tại {gameObject.name}. Đang tạo ID mới...");
                    GenerateUniqueID();
                    break;
                }
            }
        }
    }

    // Gắn thêm nút này để bạn có thể Click chuột phải vào component và tự tạo lại ID nếu thích
    [ContextMenu("Force Generate New ID")]
    private void GenerateUniqueID()
    {
        _id = $"Shrine_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
        UnityEditor.EditorUtility.SetDirty(this); // Đánh dấu để Unity lưu lại thay đổi
    }
#endif
    #endregion

    #region IInteractable
    public bool CanInteract() => _canInteract;

    public void OnInteract(bool on = true)
    {
        if (!CanInteract()) return;
        SaveManager.Instance.activeShrine = on ? this : null;
        UIHelper.AnimateZoom(saveInstruction.gameObject, on);
        UIHelper.AnimateZoom(_visualIndicator, !on);
    }

    public void Interact()
    {
        SaveManager.Instance.ExcuteSave();
    }
    #endregion

    public void DisableShrine()
    {
        _canInteract = false;
        GetComponent<SpriteRenderer>().color = Color.gray;
        UIHelper.AnimateZoom(saveInstruction.gameObject, false);
        ShowIndicator(false);
    }

    public void EnableShrine()
    {
        _canInteract = true;
        GetComponent<SpriteRenderer>().color = Color.white;
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
        if (_visualIndicator != null)
        {
            UIHelper.AnimateZoom(_visualIndicator, show && _canInteract);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _indicateRadius);
    }
    #endregion
}