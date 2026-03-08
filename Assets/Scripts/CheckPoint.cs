using UnityEngine;

public class CheckPoint : MonoBehaviour, IInteractable
{
    public string CheckpointID { get; private set; }
    public bool IsInteracted { get; private set; } = false;
    public Vector3 RespawnPosition => transform.position + Vector3.right;

    [SerializeField] private SpriteRenderer _spriteRenderer;

    void Start()
    {
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (string.IsNullOrEmpty(CheckpointID))
        {
            CheckpointID = GlobalHelper.GenerateUniqueID(gameObject);
        }
    }

    public bool CanInteract()
    {
        if (GeneralSetting.Instance.autoCheckPoint)
        {
            Interact();
        }
        return !IsInteracted;
    }

    public void Interact()
    {
        IsInteracted = true;
        _spriteRenderer.color = Color.green; // Change color to indicate checkpoint is activated
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        if (player != null)
        {
            player.UpdateCheckPoint(this); // Update player's last checkpoint
        }
    }

    public void OnInteract(bool on = true)
    {
        //animation or cái gì đó cũng được
        transform.localScale = on ? Vector3.one * 1.2f : Vector3.one; // Simple scale animation when player is in range
    }

    public void SetInteracted(bool interacted)
    {
        IsInteracted = interacted;
        _spriteRenderer.color = interacted ? Color.green : Color.white; // Reset color if checkpoint is deactivated
    }
}
