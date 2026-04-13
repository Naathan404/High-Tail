using UnityEngine;
using UnityEngine.Events;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Trigger Setting")]
    [SerializeField] private BoxCollider2D _collider;
    [SerializeField] private DialogueData _dialogueData;
    [SerializeField] private Transform _npcTransform;
    [SerializeField] private bool _isActivated = false;

    [Header("Camera Collider")]
    [SerializeField] private BoxCollider2D _confiderCollider;

    [Header("Event Callback")]
    public UnityEvent OnDialogueCompleted;


    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
    }

    [System.Obsolete]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(_isActivated) return;
        if(collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if(player != null)
            {
                player.Rb.linearVelocity = Vector2.zero;
            }
            CameraManager.Instance.SwitchRoom(_confiderCollider);
            DialogueManager.Instance.StartDialogue(
                _dialogueData,
                _npcTransform,
                () =>
                {
                    OnDialogueCompleted?.Invoke();
                });
            _isActivated = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.purple;
        Gizmos.DrawWireCube((Vector2)transform.position + _collider.offset, _collider.size);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position + _confiderCollider.offset, _confiderCollider.size);
    }
}
