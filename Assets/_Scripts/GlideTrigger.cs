using System;
using Unity.VisualScripting;
using UnityEngine;

public class GlideTrigger : MonoBehaviour
{
    public event Action OnGlideStart;
    public event Action OnGlideUpdate;
    public event Action OnGlideEnd;

    private bool _isPlayerOnGlideTrigger = false;
    private PlayerController _player;
    private bool _isCurrentlyGliding = false;
    public bool onWallGlideState = false;

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (_isPlayerOnGlideTrigger == false) return;
        if (_player == null)
        {
            Debug.LogWarning("No player controller");
            return;
        }

        onWallGlideState = (_player.StateMachine.CurrentState == _player.WallSlideState);

        if (onWallGlideState && !_isCurrentlyGliding)
        {
            _isCurrentlyGliding = true;
            Debug.Log("Start Gliding" + onWallGlideState);
            OnGlideStart?.Invoke();
        }
        else if (!onWallGlideState && _isCurrentlyGliding)
        {
            StopGliding();
        }

        if (onWallGlideState)
        {
            Debug.Log("Update Gliding");
            OnGlideUpdate?.Invoke();
        }
    }

    private void StopGliding()
    {
        Debug.Log("Stop Gliding");
        _isCurrentlyGliding = false;
        OnGlideEnd?.Invoke();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision == null) return;
        if (collision.CompareTag("Player"))
        {
            _isPlayerOnGlideTrigger = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == null) return;
        if (collision.CompareTag("Player"))
        {
            _isPlayerOnGlideTrigger = false;
            if (_isCurrentlyGliding)
            {
                StopGliding();
            }
        }
    }
}