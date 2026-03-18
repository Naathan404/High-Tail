using UnityEngine;
using System.Collections;

public class Pinecone : MonoBehaviour, IHarmful
{
    [SerializeField] private float _fallDelay;
    [SerializeField] private float _coolDown;
    [SerializeField] private Trigger _trigger;
    [SerializeField] private GameObject _pineconeDebris;
    [field: SerializeField] public int Damage { get; set; } = 6;
    [field: SerializeField] public float Knockback { get; set; } = 2.5f;

    private Renderer _renderer;
    private Collider2D _collider;
    private Rigidbody2D _rigidbody;
    private Vector3 _originalPosition;
    private bool _isCoolDown;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _renderer = GetComponent<Renderer>();
        _renderer.enabled = false;
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.bodyType = RigidbodyType2D.Static;
        _originalPosition = transform.position; // The start position will be the reset position
        _isCoolDown = true;
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
        if (_isCoolDown)
        {
            StartCoroutine(Appear());
        }
    }
    private IEnumerator Appear()
    {
        _renderer.enabled = true;
        _collider.enabled = true;
        yield return new WaitForSeconds(_fallDelay);
        Fall();
    }

    private void Fall()
    {
        _isCoolDown = false;
        _rigidbody.bodyType = RigidbodyType2D.Dynamic;
    }

    void IHarmful.DealDamage()
    {
        Break();
        StartCoroutine(ResetCoroutine());
    }

    private void Break() // When the pine cone hit the player or the ground, it break into debris
    {
        Debug.Log("<color=red> break </color>");
        // Disable the renderer & collider
        _renderer.enabled = false;
        _collider.enabled = false;
        // Spawn the debris 
        GameObject debris = Instantiate(_pineconeDebris, this.transform.position, Quaternion.identity);
    }

    private void ResetTrap() // Return to the original state after hit smth (player, ground...) and reach the cool down time;
    {
        Debug.Log("<color=green> reset pinecone trap </color>");
        _rigidbody.bodyType = RigidbodyType2D.Static;
        this.transform.position = _originalPosition;
        this.transform.rotation = Quaternion.identity;
    }

    private IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(_coolDown);
        _isCoolDown = true;
        ResetTrap();
    }

    // Vo khi cham dat
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision != null && collision.gameObject.CompareTag("Ground"))
        {
            Break();
            StartCoroutine(ResetCoroutine());
        }
    }
}
