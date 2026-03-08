using UnityEngine;

public partial class PlayerController
{
    [Header("Fall Damage")]
    [SerializeField] private float _fallDamageThreshold = -15f; // Ngưỡng vận tốc rơi để bị mất máu (Số âm vì rơi xuống)
    [SerializeField] private int _fallDamageAmount = 2;        // Lượng máu mất đi

    private bool _wasGrounded;       // Lưu trạng thái chạm đất của frame trước
    private float _lastFallVelocity; // Lưu vận tốc rơi cuối cùng trước khi chạm đất

    private void OnLanded()
    {
        Debug.Log($"Tiếp đất! Vận tốc rơi cuối cùng là: {_lastFallVelocity}");

        // Kiểm tra xem rơi có đủ mạnh không (Nhớ là vận tốc rơi xuống mang giá trị âm)
        if (_lastFallVelocity <= _fallDamageThreshold)
        {
            ApplyHP(-_fallDamageAmount); // Trừ máu
            OnPlayerHardLanded.Invoke();
        }

        _lastFallVelocity = 0f;
    }
}
