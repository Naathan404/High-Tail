using UnityEngine;

public class CheckPoint : MonoBehaviour, IInteractable
{
    public bool IsInteracted = false;
    public Vector3 RespawnPosition => transform.position + Vector3.right;

    [SerializeField] private SpriteRenderer _spriteRenderer;

    void Start()
    {
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
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
    }

    public void OnInteract(bool on = true)
    {
        //animation or cái gì đó cũng được
        transform.localScale = on ? Vector3.one * 1.2f : Vector3.one; // Simple scale animation when player is in range
    }
}
