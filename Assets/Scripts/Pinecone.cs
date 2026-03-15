using UnityEngine;
using System.Collections;

public class Pinecone : MonoBehaviour, IHarmful
{
    [SerializeField] private float _fallDelay;
    //[SerializeField] private float _fallTime;
    [field: SerializeField] public int Damage { get; set; } = 6;
    [field: SerializeField] public float Knockback { get; set; } = 2.5f;

    private Trigger _trigger;
    private Renderer _renderer;
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _trigger = GetComponentInChildren<Trigger>();
        _renderer = GetComponent<Renderer>();
        _renderer.enabled = false;
        _rigidbody = GetComponentInChildren<Rigidbody2D>();
        _rigidbody.bodyType = RigidbodyType2D.Static;
    }

    private void OnEnable()
    {
        _trigger.OnStepIn += DoSomething;
    }

    private void OnDisable()
    {
        _trigger.OnStepIn -= DoSomething;
    }

    private void DoSomething()
    {
        StartCoroutine(Appear());
    }
    private IEnumerator Appear()
    {
        _renderer.enabled = true;
        yield return new WaitForSeconds(_fallDelay);
        Fall();
    }

    private void Fall()
    {
        _rigidbody.bodyType = RigidbodyType2D.Dynamic;
    }

    void IHarmful.DealDamage()
    {
        //
    }
}
