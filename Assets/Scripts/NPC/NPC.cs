using System.Collections;
using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    public DialogueData[] dialogueData;
    public GameObject interactIcon;
    public int dialogueIndex = 0;

    void Start()
    {
        if (interactIcon != null)
        {
            interactIcon.SetActive(false);
        }
    }
    public bool CanInteract()
    {
        return dialogueIndex < dialogueData.Length;
    }

    public void Interact()
    {
        if (dialogueData == null || PauseGameManager.IsGamePaused)
        {
            return;
        }
        DialogueManager.Instance.StartDialogue(dialogueData[dialogueIndex++]);
        interactIcon.SetActive(CanInteract());
    }

    public void OnInteract(bool on = true)
    {
        interactIcon.SetActive(on);
    }
}
