using UnityEngine;

// Use this script to move the force field to along the player
public class ForceField : MonoBehaviour
{
    [SerializeField] private GameObject _player;

    private void Awake()
    {
        MoveToPlayer();
    }

    private void LateUpdate()
    {
        MoveToPlayer();
    }

    private void MoveToPlayer()
    {
        if (_player == null)
        {
            Debug.LogWarning("Player reference is missing in ForceField script.");
            return;
        }

        transform.position = _player.transform.position;
    }
}