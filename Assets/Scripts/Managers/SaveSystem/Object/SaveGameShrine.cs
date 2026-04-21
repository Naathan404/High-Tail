using UnityEngine;

public class SaveGameShrine : MonoBehaviour, IInteractable
{
    private bool _canInteract = true;
    [SerializeField] private string _id;
    public string ID => _id;

    private void Start()
    {
        EnableShrine();
    }

    private void OnEnable()
    {
        #if UNITY_EDITOR
        if (string.IsNullOrEmpty(_id))
        {
            // Tự động sinh ID dựa trên tên và một mã ngẫu nhiên để đảm bảo duy nhất
            _id = $"{gameObject.name}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
            
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            
            Debug.Log($"[Auto-Save] Đã 'khắc' ID vĩnh viễn cho {gameObject.name}: {_id}");
        }
        #endif
    }

    #region IInteratable
    public bool CanInteract() => _canInteract;
    public void OnInteract(bool on = true)
    {
        if (!CanInteract()) return;

        SaveManager.Instance.ShowSaveOption(on);
        SaveManager.Instance.activeShrine = on ? this : null;
    }

    public void Interact()
    {
        SaveManager.Instance.ExcuteSave();
    }

    #endregion

    public void DisableShrine()
    {
        _canInteract = false;
        GetComponent<SpriteRenderer>().color =Color.gray;
    }
    public void EnableShrine()
    {
        _canInteract = true;
        GetComponent<SpriteRenderer>().color = Color.white;
    }
}