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

            if (lastCheckPoint != null)
            {
                transform.position = lastCheckPoint.RespawnPosition;
                // Reset player state if needed (e.g., health, inventory)
            }
            else
            {
                // Handle case where no checkpoint is set (e.g., respawn at start position)
                transform.position = Vector3.zero; // Example: respawn at origin
            }

            Debug.Log("Bé đã hồi xuân");
        }
    }

}
