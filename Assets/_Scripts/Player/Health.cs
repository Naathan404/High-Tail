using System.Collections;
using DG.Tweening;
using UnityEngine;

public partial class PlayerController
{
    [Header("Fall Damage")]
    [SerializeField] private float _fallDamageThreshold = -20f; // Ngưỡng vận tốc rơi để bị mất máu (Số âm vì rơi xuống)
    [SerializeField] private int _fallDamageAmount = 1;        // Lượng máu mất đi
    [SerializeField] private float _timeShakingDuration = 0.5f;

    private bool _wasGrounded;       // Lưu trạng thái chạm đất của frame trước
    private float _lastFallVelocity; // Lưu vận tốc rơi cuối cùng trước khi chạm đất
    private bool _ignoreNextFallDamage = false;

    private void OnLanded()
    {
        if (_lastFallVelocity <= _fallDamageThreshold)
        {
            if (_ignoreNextFallDamage)
            {
                _ignoreNextFallDamage = false;
            }
            else
            {
                Debug.Log("Rơi quá cao, gãy chân rồi!");
                OnPlayerHardLanded?.Invoke();
            }
        }
        _lastFallVelocity = 0f;
    }

    #region HP and Energy
    public void KillPlayer()
    {
        UIManager.Instance.AddDeathCount();
        if (_stateMachine.CurrentState == DeathState) return;

        _stateMachine.ChangeState(DeathState);
        OnPlayerDamaged?.Invoke(); 
        OnPlayerDied?.Invoke(); 

        StartCoroutine(DeathSequenceCo());
    }

    private IEnumerator DeathSequenceCo()
    {
        DeathScreenManager.Instance.TriggerDeathWipe(
            transform.position, 
            CheckpointManager.Instance.CurrentSpawnPoint.position,
            () =>
            {
                CheckpointManager.Instance.RespawnPlayer(this);
                _stateMachine.ChangeState(IdleState);
            },
            Data.respawnDuration
        );
        yield return null;
        
    }

    public void ApplyEnergy(int amount)
    {
        if (amount >= 0)
            Debug.Log($"Người chơi hồi {amount} năng lượng");
        else
            Debug.Log($"Người chơi tiêu hao {amount} năng lượng");
    }

    // public void SetHP(int hp)
    // {
    //     _hp = Mathf.Clamp(hp, 0, Data.maxHP);
    // }

    // public void SetEnergy(int energy)
    // {
    //     _energy = Mathf.Clamp(energy, 0, Data.maxEnergy);
    // }

    #endregion
}
