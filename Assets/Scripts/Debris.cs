using UnityEngine;
using System.Collections;

public class Debris : MonoBehaviour
{
    [SerializeField] private float _despawnTime; // The time this debris will be despawned after it is spawned
    [SerializeField] private float _force; // The force that push out the pieces after gameObject is broken
    [SerializeField] private float _torque;
    private int _count; // Sum of the pieces 

    private void Awake()
    {
        _count = this.transform.childCount;
    }

    private void Start()
    {
        Explode();
    }

    private void Explode()
    {
        Rigidbody2D[] childRb = GetComponentsInChildren<Rigidbody2D>();
        Vector3 forcePosition = transform.position;
        foreach (var rb in childRb)
        {
            // Offset for the explosion force
            float randX = Random.Range(1f, 4f);
            float randY = Random.Range(1f, 4f);

            Vector2 force = new Vector2(_force + randX, _force + randY);
            rb.AddForceAtPosition(force, forcePosition, ForceMode2D.Impulse);
            rb.AddTorque(_torque);
        }

        // Destroy debris
        StartCoroutine(DestroyDebris());
    }

    private IEnumerator DestroyDebris()
    {
        yield return new WaitForSeconds(_despawnTime);
        Destroy(this.gameObject);
    }
}
