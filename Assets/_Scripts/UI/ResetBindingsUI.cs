using UnityEngine;
using UnityEngine.UI;

public class ResetBindingsUI : MonoBehaviour
{
    [SerializeField] private Button _resetButton;

    private void Start()
    {
        if (_resetButton != null)
        {
            _resetButton.onClick.AddListener(ResetToDefault);
        }
    }

    private void ResetToDefault()
    {
        InputManager.Instance.ResetAllBindingsToDefault();
        AudioManager.Instance.PlaySFX(SoundName.UI_Click_Button);

        foreach (var btn in FindObjectsByType<RebindButtonUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            btn.GetComponent<RebindButtonUI>()?.SendMessage("ClearAssetOverrides", SendMessageOptions.DontRequireReceiver);
        }

        if (SaveManager.Instance != null && SaveManager.Instance.MainData != null)
        {
            SaveManager.Instance.MainData.settings.keyBindings = string.Empty;
            SaveManager.Instance.SaveToDisk();
        }

        RebindButtonUI[] allRebindButtons = FindObjectsByType<RebindButtonUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var btn in allRebindButtons)
        {
            btn.UpdateButtonText();
        }

        Debug.Log("Đã khôi phục toàn bộ phím về mặc định!");
    }
}