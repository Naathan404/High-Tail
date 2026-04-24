using UnityEngine;
using System.Collections;

public class PineconeTrap : MonoBehaviour
{
    [SerializeField] private GameObject _pineconePrefab;
    [SerializeField] private float _spawnDelay;
    [SerializeField] private Trigger _spawnTrigger;
    [SerializeField] private Trigger _stopTrigger;
    private Coroutine _spawnRoutine;

    private void OnEnable()
    {
        _spawnTrigger.OnStepIn += SpawningPinecone;
        _stopTrigger.OnStepIn += Stop;
    }

    private void OnDisable()
    {
        _spawnTrigger.OnStepIn -= SpawningPinecone;
        _stopTrigger.OnStepIn -= Stop;
    }

    private void SpawningPinecone()
    {
        if (_spawnRoutine == null)
        {
            _spawnRoutine = StartCoroutine(SpawnRoutine());
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            Instantiate(_pineconePrefab, transform.position, Quaternion.identity);
            yield return new WaitForSeconds(_spawnDelay);
        }
    }

    private void Stop()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }
    }
}
