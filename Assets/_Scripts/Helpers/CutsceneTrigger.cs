using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CutsceneTrigger : MonoBehaviour
{
    [SerializeField] private PlayableDirector _director;
    [SerializeField] private PlayerController _player;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            _player.CanMove = false;
            _player.Rb.gravityScale = 0f;
            _player.StateMachine.ChangeState(_player.IdleState);
            _director.Play();
            _director.stopped += OnCutsceneEnded;
            GetComponent<Collider2D>().enabled = false;
        }
    }

    private void OnCutsceneEnded(PlayableDirector obj)
    {
        _player.CanMove = true;
        _director.stopped -= OnCutsceneEnded;
    }
}
