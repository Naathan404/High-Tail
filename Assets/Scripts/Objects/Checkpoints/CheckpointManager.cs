using System;
using DG.Tweening;
using UnityEngine;

public class CheckpointManager : Singleton<CheckpointManager>
{
    [SerializeField] private Transform _defaultSpawnPoint;
    public Transform CurrentSpawnPoint { get; private set; }
    private bool _isFacingRight;
    

    private void Start()
    {
        CurrentSpawnPoint = _defaultSpawnPoint;
    }

    private void OnEnable()
    {
        HotCheckpoint.OnTriggerNewCheckpoint += SetSpawnPoint;
    }

    private void OnDisable()
    {
        HotCheckpoint.OnTriggerNewCheckpoint -= SetSpawnPoint;
    }

    public void SetSpawnPoint(Transform spawnPoint, bool isFacingRight)
    {
        CurrentSpawnPoint = spawnPoint;
        _isFacingRight = isFacingRight;
    }

    public void RespawnPlayer(PlayerController player)
    {
        player.transform.position = CurrentSpawnPoint.position;
        player.Rb.linearVelocity = Vector2.zero;
        float right = _isFacingRight ? 1f : -1f;
        player.transform.localScale = new Vector2(player.transform.localScale.x * right, player.transform.localScale.y);
    }
}
