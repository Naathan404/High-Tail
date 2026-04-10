using System;
using UnityEngine;

public class CheckpointManager : Singleton<CheckpointManager>
{
    [SerializeField] private Transform _defaultSpawnPoint;
    public Transform CurrentSpawnPoint { get; private set; }

    private void Start()
    {
        CurrentSpawnPoint = _defaultSpawnPoint;
    }

    public void SetSpawnPoint(Transform spawnPoint)
    {
        CurrentSpawnPoint = spawnPoint;
    }
}
