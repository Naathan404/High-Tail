using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerController : MonoBehaviour
{
    [Header("Respawn Settings")]
    public CheckPoint lastCheckPoint;

    public void UpdateCheckPoint(CheckPoint newCheckPoint)//Used by checkpoint when player interacts with it
    {
        lastCheckPoint = newCheckPoint;
    }

    public void Respawn(InputAction.CallbackContext context) //R
    {
        if (context.started)
        {

            Respawn();
        }
    }

    private void Respawn() //Used for auto respawn when player falls into a pit or something like that
    {
        if (lastCheckPoint != null)
        {
            transform.position = lastCheckPoint.RespawnPosition;
            if (_hp <= 0)
            {
                _hp = Data.maxHP; // Reset HP to max when respawning
            }
        }
        else
        {
            // Handle case where no checkpoint is set (e.g., respawn at start position)
            transform.position = Vector3.zero; // Example: respawn at origin
        }

        Debug.Log("Bé đã hồi xuân");
    }

}
