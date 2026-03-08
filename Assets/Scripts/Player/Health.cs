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
            OnPlayerHardLanded?.Invoke();
        }

        _lastFallVelocity = 0f;
    }

    #region HP and Energy
    public void ApplyHP(int amount)
    {
        _hp = Mathf.Clamp(_hp + amount, 0, Data.maxHP);
        if (amount >= 0)
        {
            OnPlayerHealed?.Invoke();
            Debug.Log($"Hồi {_hp} máu cho player");
        }
        else
        {
            OnPlayerDamaged?.Invoke();
            Debug.Log("Người chơi nhận damage");
            if (_hp <= 0)
            {
                Debug.Log("Bé đã chết");
                OnPlayerDied?.Invoke();
            }
        }
    }

    public void ApplyEnergy(int amount)
    {
        _energy = Mathf.Clamp(_energy + amount, 0, Data.maxEnergy);
        if (amount >= 0)
            Debug.Log($"Người chơi hồi {amount} năng lượng");
        else
            Debug.Log($"Người chơi tiêu hao {amount} năng lượng");
    }

    public void SetHP(int hp)
    {
        _hp = Mathf.Clamp(hp, 0, Data.maxHP);
    }

    public void SetEnergy(int energy)
    {
        _energy = Mathf.Clamp(energy, 0, Data.maxEnergy);
    }

    #endregion
}
