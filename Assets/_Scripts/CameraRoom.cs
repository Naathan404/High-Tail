using System;
using UnityEngine;

public class CameraRoom : MonoBehaviour
{
    public BoxCollider2D _confinerCollider;
    [SerializeField] private int _roomFOV = 80;

    private void Awake()
    {
        if(_confinerCollider == null)
            _confinerCollider = GetComponent<BoxCollider2D>();
    }

    [System.Obsolete]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Khi sóc Tail chạm vào biên giới phòng mới
        if (collision.CompareTag("Player"))
        {
            CameraManager.Instance.CurrentBoundary = _confinerCollider;
            // Gọi CameraManager để đổi phòng
            CameraManager.Instance.SwitchRoom(_confinerCollider, _roomFOV);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector2 center = (Vector2)transform.position + _confinerCollider.offset;
        Vector2 size = _confinerCollider.size;
        Gizmos.DrawWireCube(center, size);
    }
}
