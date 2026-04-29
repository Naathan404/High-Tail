using UnityEngine;
using DG.Tweening;
using UnityEngine.PlayerLoop; // Dùng để làm Sóc bẹp dúm

public class CrusherDamage : MonoBehaviour
{
    public bool IsDangerous = false; 
    public bool IsVerticle = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsDangerous && collision.CompareTag("Player"))
        {
            // PlayerController player = collision.GetComponent<PlayerController>();
            // if (player != null)
            // {
            //     if(player.IsOnGround())
            //     {
            //         collision.transform.DOScale(new Vector3(1.5f, 0.2f, 1f), 0.1f);
            //         player.KillPlayer();
            //     }
            //     else
            //     {
            //         collision.transform.DOShakePosition(0.2f, strength: 0.3f);
            //     }
            // }
            // else
            // {
            //     Debug.LogWarning("Player object does not have a PlayerController component.");
            // }
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                if(IsVerticle)
                    collision.transform.DOScale(new Vector3(1.1f, 0.9f, 1f), 0.1f);
                else
                    collision.transform.DOScale(new Vector3(0.9f, 1.1f, 1f), 0.1f);
                player.KillPlayer();
            }
        }
    }

        private void OnTriggerStay2D(Collider2D collision)
    {
        if (IsDangerous && collision.CompareTag("Player"))
        {
            // PlayerController player = collision.GetComponent<PlayerController>();
            // if (player != null)
            // {
            //     if(player.IsOnGround())
            //     {
            //         collision.transform.DOScale(new Vector3(1.5f, 0.2f, 1f), 0.1f);
            //         player.KillPlayer();
            //     }
            //     else
            //     {
            //         collision.transform.DOShakePosition(0.2f, strength: 0.3f);
            //     }
            // }
            // else
            // {
            //     Debug.LogWarning("Player object does not have a PlayerController component.");
            // }
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                if(IsVerticle)
                    collision.transform.DOScale(new Vector3(1.1f, 0.9f, 1f), 0.1f);
                else
                    collision.transform.DOScale(new Vector3(0.9f, 1.1f, 1f), 0.1f);
                player.KillPlayer();
            }
        }
    }
}