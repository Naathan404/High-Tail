using UnityEngine;
public interface IInteractable
{
    public void Interact();
    public bool CanInteract();

    public void OnInteract(bool on = true);
}
