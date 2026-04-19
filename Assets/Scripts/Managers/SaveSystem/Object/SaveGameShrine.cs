using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveGameShrine : MonoBehaviour, IInteractable
{
    public string ShrineID {get; private set;}
    public bool IsSaved {get; private set;}
    public bool CanInteract() => !IsSaved;

    public void Interact()
    {
        SaveManager.Instance.ExcuteSave();
        Debug.Log("Shrine Interact");
    }

    public void OnInteract(bool on = true)
    {
        SaveManager.Instance.ShowSaveOption(on);
        if (on) SaveManager.Instance.activeShrine = this;
        else SaveManager.Instance.activeShrine = null;
    }
}
