using UnityEngine;

public class CameraRoom : MonoBehaviour
{
private BoxCollider2D _confinerCollider;

    private void Awake()
    {
        _confinerCollider = GetComponent<BoxCollider2D>();
    }

    [System.Obsolete]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Khi sóc Tail chạm vào biên giới phòng mới
        if (collision.CompareTag("Player"))
        {
            // Gọi CameraManager để đổi phòng
            CameraManager.Instance.SwitchRoom(_confinerCollider);
        }
    }
}
