using System.Collections;
using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    public string NPCID { get; private set; }
    public DialogueData[] dialogueData;
    public GameObject interactIcon;
    public int dialogueIndex = 0;

    void Start()
    {
        if (interactIcon != null)
        {
            interactIcon.SetActive(false);
        }
        if (string.IsNullOrEmpty(NPCID))
        {
            NPCID = GlobalHelper.GenerateUniqueID(gameObject);
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
