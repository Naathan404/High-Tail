using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    //Để dành sau này làm button Interact, hiện tại thì bấm phím E
    //[SerializeField] private GameObject interactionIcon;

    private void Start()
    {
        InputManager.Instance.Inputs.Interaction.Interact.started += OnInteract;
    }

    private void OnDestroy()
    {
        InputManager.Instance.Inputs.Interaction.Interact.started -= OnInteract;
    }



    public void OnInteract(InputAction.CallbackContext context)//Dùng PlayerInput để gọi hàm này khi bấm phím Interact (E)
    {
        if (context.started)
        {
            if (interactableInRange != null)
            {
                interactableInRange.Interact();
                if (!interactableInRange.CanInteract())
                {
                    //if (interactionIcon != null)
                    //    interactionIcon.SetActive(false);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable)
            && interactable.CanInteract())
        {
            interactableInRange = interactable;
            interactableInRange.OnInteract(true);
            //interactionIcon.SetActive(true);
            //SoundEffectManager.Play("Interact");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable)
            && interactable == interactableInRange)
        {
            interactableInRange.OnInteract(false);
            interactableInRange = null;
            //interactionIcon.SetActive(false);
        }
    }
}
