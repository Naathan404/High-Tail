using System;
using DG.Tweening;
using UnityEngine;

public class CheckpointManager : Singleton<CheckpointManager>
{
    [SerializeField] private Transform _defaultSpawnPoint;
    public Transform CurrentSpawnPoint { get; private set; }

    

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

    public void SetSpawnPoint(Transform spawnPoint)
    {
        CurrentSpawnPoint = spawnPoint;
    }

    public void RespawnPlayer(PlayerController player)
    {
        player.transform.position = CurrentSpawnPoint.position;
        player.Rb.linearVelocity = Vector2.zero;
    }
}
