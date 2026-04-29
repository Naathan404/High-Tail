using System.Runtime.CompilerServices;
using UnityEngine;

public class SaveGameShrine : MonoBehaviour, IInteractable
{
    [Header("Save settings")]
    private bool _canInteract = true;
    [SerializeField] private string _id;
    public string ID => _id;

    [Header("Detection")]
    [SerializeField] private float _radius = 2f; // Bán kính kiểm tra player
    [SerializeField] private LayerMask _playerLayer; // Layer của Player
    [SerializeField] private GameObject _visualIndicator;
    private bool _isPlayerNearby = false;

    private void Start()
    {
        EnableShrine();
        if (_visualIndicator != null) _visualIndicator.SetActive(false);
    }

    private void Update()
    {
        CheckForPlayer();
    }

    private void OnEnable()
    {
        #if UNITY_EDITOR
        if (string.IsNullOrEmpty(_id))
        {
            // Tự động sinh ID dựa trên tên và một mã ngẫu nhiên để đảm bảo duy nhất
            _id = $"{gameObject.name}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
            
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            
            Debug.Log($"[Auto-Save] Đã 'khắc' ID vĩnh viễn cho {gameObject.name}: {_id}");
        }
        #endif
    }

    #region IInteratable
    public bool CanInteract() => _canInteract;
    public void OnInteract(bool on = true)
    {
        if (!CanInteract()) return;

        SaveManager.Instance.ShowSaveOption(on);
        SaveManager.Instance.activeShrine = on ? this : null;
    }

    public void Interact()
    {
        SaveManager.Instance.ExcuteSave();
    }

    #endregion

    public void DisableShrine()
    {
        _canInteract = false;
        GetComponent<SpriteRenderer>().color =Color.gray;
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
        // Kiểm tra xem có Player trong vùng bán kính không
        Collider2D player = Physics2D.OverlapCircle(transform.position, _radius, _playerLayer);

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
            _visualIndicator.SetActive(show && _canInteract);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
    #endregion
}