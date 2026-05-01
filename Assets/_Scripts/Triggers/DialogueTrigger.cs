using UnityEngine;
using UnityEngine.Events;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Trigger Setting")]
    [SerializeField] private BoxCollider2D _collider;
    [SerializeField] private DialogueData _dialogueData;
    [SerializeField] private DialogueData _secondDaryDialogueData;
    [SerializeField] private Transform _npcTransform;
    [SerializeField] private bool _canInteract = false;
    [SerializeField] private GameObject _interactMark;
    [SerializeField] private bool _isOneTimeTrigger = false;

    [Header("Camera Collider")]
    [SerializeField] private BoxCollider2D _confiderCollider;

    [Header("Event Callback")]
    public UnityEvent OnDialogueCompleted;

    PlayerController _player;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
    }

    [System.Obsolete]
    private void Update()
    {
        _interactMark.SetActive(_canInteract);
        if(!_canInteract) return;
        if(InputManager.Instance.Inputs.Interaction.Interact.WasPressedThisFrame())
        {
            _player.Rb.linearVelocity = Vector2.zero;
            CameraManager.Instance.SwitchRoom(_confiderCollider, 50, false, 80f, true);
            _canInteract = false;
            DialogueManager.Instance.StartDialogue(
                _dialogueData.IsActivated && !_isOneTimeTrigger ? _secondDaryDialogueData : _dialogueData,
                _npcTransform,
                () =>
                {
                    OnDialogueCompleted?.Invoke();
                    if(_isOneTimeTrigger)
                    {
                        _canInteract = false;
                    }
                    else
                    {
                        _canInteract = true;
                    }
                    _dialogueData.IsActivated = true;
                });
        }
    }

    [System.Obsolete]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(_isOneTimeTrigger && _dialogueData.IsActivated) return;
        if(collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if(player != null)
            {
                if(player.StateMachine.CurrentState == player.DashState
                && player.StateMachine.CurrentState == player.JumpState)
                {
                    return;
                }

                _canInteract = true;
                _player = player;
            }


        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _canInteract = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.purple;
        Gizmos.DrawWireCube((Vector2)transform.position + _collider.offset, _collider.size);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position + _confiderCollider.offset, _confiderCollider.size);
    }
}
