using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private Button _loadButton;

    private string _nodeID;

    public void Setup(SaveNode node)
    {
        _nodeID = node.nodeID;
        
        _nameText.text = node.commitName;
        _timeText.text = node.timestamp;

        _loadButton.onClick.RemoveAllListeners();
        _loadButton.onClick.AddListener(OnLoadClicked);
    }

    private void OnLoadClicked()
    {
        SaveManager.Instance.LoadGameFromNode(_nodeID);
    }
}