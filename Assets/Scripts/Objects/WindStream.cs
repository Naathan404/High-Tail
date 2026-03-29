using UnityEngine;

public class WindStream : MonoBehaviour
{
    [Header("Wind Settings")]
    public Vector2 WindDirection = Vector2.right; 
    public float WindForce = 15f; 

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            
            if (player != null)
            {
                player.ApplyWindForce(WindDirection.normalized * WindForce);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ApplyWindForce(Vector2.zero); 
            }
        }
    }
}