using UnityEngine;
using UnityEngine.Events;

public class TaleStoneTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    private Collider2D _trigger;
    [SerializeField] private GameObject _interactMark;
    [SerializeField] private TaleStoneData _taleStoneData;
    [SerializeField] private Transform _camCenterTransform;
    [SerializeField] private TaleStoneType _type;
    [SerializeField] private GameObject _light;


    [Header("Camera Collider")]
    [SerializeField] private BoxCollider2D _confiderCollider;

    [Header("Event Callback")]
    public UnityEvent OnTaleStoneDialogueCompleted;

    private bool _canInteract = false;


    private void Awake()
    {
        _trigger = GetComponent<Collider2D>();
        _interactMark.SetActive(false);
    }

    [System.Obsolete]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            if(_type == TaleStoneType.SkillUnlock && _taleStoneData.IsActivated) return;

            _interactMark.SetActive(true);
            _canInteract = true;
        }
        
    }

    [System.Obsolete]
    private void Update()
    {
        if(_type == TaleStoneType.SkillUnlock && (!_canInteract || _taleStoneData.IsActivated )) return;
        if(_type != TaleStoneType.SkillUnlock && !_canInteract) return;

        
        // nếu _canInteract và isActivated = false
        if(InputManager.Instance.Inputs.Interaction.Interact.WasPressedThisFrame())
        {
            CameraManager.Instance.SwitchRoom(_confiderCollider, 50);
            _canInteract = false;
            TaleStoneManager.Instance.StartTale(
            _taleStoneData,
            _camCenterTransform,
            () =>
            {
                OnTaleStoneDialogueCompleted?.Invoke();
            });
            _taleStoneData.IsActivated = true;
        }
    }

    private void OnTriggerExit2D()
    {
        _interactMark.SetActive(false);
        _canInteract = false;
    }

    public enum TaleStoneType
    {
        Normal,
        Scratched,
        SkillUnlock
    }
}