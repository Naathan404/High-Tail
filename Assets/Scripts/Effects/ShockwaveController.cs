using System.Collections;
using UnityEngine;

public class ShockwaveController : MonoBehaviour
{
    [SerializeField] private float _shockWaveTime = 0.5f;
    [SerializeField] private Material _material;
    private Coroutine _shockWaveCoroutine;
    private static int _waveDistanceFromCenter = Shader.PropertyToID("_WaveDistanceFromCenter");

    private void Awake()
    {
        _material = GetComponent<SpriteRenderer>().material;
    }



    private void Update()
    {
        // if(InputManager.Instance.Inputs.Testing.ShockWave.WasPressedThisFrame())
        // {
        //     CallShockWave(Tran);
        // }
    }

    public void CallShockWave(Transform center)
    {
        // this.gameObject.SetActive(true);
        // Vector2 screenPos = Camera.main.WorldToViewportPoint(center.transform.position);
        // _material.SetVector("_ShockwaveSpawnPosition", screenPos);
        // _shockWaveCoroutine = StartCoroutine(HandleShockWave(-0.1f, 1f));
        this.transform.position = center.position; 

        this.gameObject.SetActive(true);
        _material.SetVector("_ShockwaveSpawnPosition", new Vector2(0.5f, 0.5f));
        if (_shockWaveCoroutine != null) StopCoroutine(_shockWaveCoroutine);
        _shockWaveCoroutine = StartCoroutine(HandleShockWave(-0.1f, 1f));       
    }

    private IEnumerator HandleShockWave(float startPos, float endPos)
    {
        _material.SetFloat(_waveDistanceFromCenter, startPos);

        float lerpAmount = 0f;
        float elapsedTime = 0f;

        while(elapsedTime < _shockWaveTime)
        {
            elapsedTime += Time.deltaTime;
            lerpAmount = Mathf.Lerp(startPos, endPos, elapsedTime / _shockWaveTime);
            _material.SetFloat(_waveDistanceFromCenter, lerpAmount);

            yield return null;
        }

        gameObject.SetActive(false);
    }
}
