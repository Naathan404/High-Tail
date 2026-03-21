using UnityEngine;
using System.Collections;

public class Pinecone : MonoBehaviour, IHarmful
{
    [SerializeField] private float _fallDelay;
    [SerializeField] private GameObject _pineconeDebris;
    [field: SerializeField] public int Damage { get; set; } = 6;
    [field: SerializeField] public float Knockback { get; set; } = 2.5f;
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.bodyType = RigidbodyType2D.Static;
        StartCoroutine(Appear());
    }

    private IEnumerator Appear()
    {
        yield return new WaitForSeconds(_fallDelay);
        Fall();
    }

    private void Fall()
    {
        _rigidbody.bodyType = RigidbodyType2D.Dynamic;
    }

    void IHarmful.DealDamage()
    {
        Break();
    }

    private void Break() // When the pine cone hit the player or the ground, it break into debris
    {
        Debug.Log("<color=red> break </color>");
        // Spawn the debris first
        GameObject debris = Instantiate(_pineconeDebris, this.transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }

    // Vo khi cham dat
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision != null && collision.gameObject.CompareTag("Ground"))
        {
            Break();
        }
    }
}
