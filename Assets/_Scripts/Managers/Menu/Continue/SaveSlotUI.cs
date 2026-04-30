using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private Button _loadButton;
    [SerializeField] private Button _deleteButton;

    private string _nodeID;

    public void Setup(SaveNode node)
    {
        _nodeID = node.nodeID;
        
        _nameText.text = node.commitName;
        _timeText.text = node.timestamp;

        _loadButton.onClick.RemoveAllListeners();
        _loadButton.onClick.AddListener(OnLoadClicked);
        _deleteButton.onClick.AddListener(OnDeleteClicked);
    }

    private void OnLoadClicked()
    {
        SaveManager.Instance.LoadGameFromNode(_nodeID);
        MenuManager.Instance.CloseAllMenus();
    }

    private void OnDeleteClicked()
    {
        SaveManager.Instance.DeleteSaveNode(_nodeID);
    }
}