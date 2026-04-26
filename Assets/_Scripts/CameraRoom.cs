using Unity.Cinemachine;
using UnityEngine;

public class CameraRoom : MonoBehaviour
{
    public BoxCollider2D _confinerCollider;
    
    [Header("Room Camera Settings")]
    [SerializeField] private float _roomBaseFOV = 80f;

    public bool isDynamicCamera = false;
    public float dashFOV = 75f;

    private void Awake()
    {
        if(_confinerCollider == null)
            _confinerCollider = GetComponent<BoxCollider2D>();
    }

    [System.Obsolete]
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && _confinerCollider.OverlapPoint(player.transform.position))
        {
            CameraManager.Instance.ForceUpdateRoomSettings(_roomBaseFOV, isDynamicCamera, dashFOV);
            CameraManager.Instance.CurrentBoundary = _confinerCollider;
            
            // Cập nhật Confiner
            var confiner = CameraManager.Instance.GetComponentInChildren<CinemachineConfiner2D>();
            if (confiner != null) {
                confiner.BoundingShape2D = _confinerCollider;
                confiner.InvalidateCache();
            }
        }
    }    

    [System.Obsolete]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CameraManager.Instance.CurrentBoundary = _confinerCollider;
            
            CameraManager.Instance.SwitchRoom(_confinerCollider, _roomBaseFOV, isDynamicCamera, dashFOV);
        }
    }

    private void OnDrawGizmos()
    {
        if (_confinerCollider == null) return;
        Gizmos.color = isDynamicCamera ? Color.yellow : Color.red; // Đổi màu Gizmo để dễ nhận biết phòng Dynamic
        Vector2 center = (Vector2)transform.position + _confinerCollider.offset;
        Vector2 size = _confinerCollider.size;
        Gizmos.DrawWireCube(center, size);
    }
}