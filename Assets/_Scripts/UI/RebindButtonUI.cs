using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class RebindTarget
{
    public InputActionReference actionRef;
    public int bindingIndex = 0;
}

public class RebindButtonUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button _rebindButton;
    [SerializeField] private TextMeshProUGUI _buttonText; 
    
    [Header("Action Setup")]
    [SerializeField] private List<RebindTarget> _targets; 

    private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;

    private void Start()
    {
        _rebindButton.onClick.AddListener(StartRebinding);
        
        // Delay nhẹ 0.1s để chờ SaveManager gọi hàm LoadBindingOverrides nạp phím từ JSON xong rồi mới Update mặt chữ
        Invoke(nameof(UpdateButtonText), 0.1f); 
    }

    private InputAction GetLiveAction(InputActionReference refAction)
    {
        if (refAction == null || InputManager.Instance == null || InputManager.Instance.Inputs == null) 
            return null;
        
        // Trỏ trực tiếp vào Inputs.asset của InputManager thay vì xài file gốc
        return InputManager.Instance.Inputs.asset.FindAction(refAction.action.id);
    }

    private void StartRebinding()
    {
        if (_targets == null || _targets.Count == 0) return;
        AudioManager.Instance.PlaySFX(SoundName.UI_Click_Button);

        var primaryLiveAction = GetLiveAction(_targets[0].actionRef);
        if (primaryLiveAction == null) return;

        _rebindButton.interactable = false;
        _buttonText.text = "..."; 

        // Tắt action ĐANG CHẠY để lắng nghe phím mới
        foreach (var target in _targets) 
        {
            var liveAction = GetLiveAction(target.actionRef);
            if (liveAction != null) liveAction.Disable();
        }

        // Bắt đầu quá trình Rebind TRỰC TIẾP trên bản sao
        _rebindingOperation = primaryLiveAction.PerformInteractiveRebinding(_targets[0].bindingIndex)
            .WithExpectedControlType("Button")
            .WithControlsExcluding("Mouse")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation => CompleteRebind())
            .OnCancel(operation => CancelRebind())
            .Start();
    }

    private void CompleteRebind()
    {
        if (_targets == null || _targets.Count == 0) return;

        var primaryLiveAction = GetLiveAction(_targets[0].actionRef);
        string newPath = primaryLiveAction.bindings[_targets[0].bindingIndex].effectivePath; 

        // Gán phím cho các action gom chung (Ví dụ: Nút Tương tác gộp với Nhìn lên)
        for (int i = 1; i < _targets.Count; i++)
        {
            var otherLiveAction = GetLiveAction(_targets[i].actionRef);
            if (otherLiveAction != null)
            {
                otherLiveAction.ApplyBindingOverride(_targets[i].bindingIndex, newPath);
            }
        }

        CleanUpOperation();
    }

    private void CancelRebind()
    {
        CleanUpOperation();
    }

    private void CleanUpOperation()
    {
        // Mở lại các action ĐANG CHẠY
        foreach (var target in _targets) 
        {
            var liveAction = GetLiveAction(target.actionRef);
            if (liveAction != null) liveAction.Enable();
        }

        _rebindingOperation?.Dispose();
        _rebindingOperation = null;
        _rebindButton.interactable = true;
        
        UpdateButtonText();
        SaveBindings(); // Gọi save JSON
    }

    public void UpdateButtonText()
    {
        if (_targets != null && _targets.Count > 0)
        {
            var primaryLiveAction = GetLiveAction(_targets[0].actionRef);
            if (primaryLiveAction != null)
            {
                // Lấy đường dẫn phím thực tế ĐANG CHẠY để hiển thị
                _buttonText.text = InputControlPath.ToHumanReadableString(
                    primaryLiveAction.bindings[_targets[0].bindingIndex].effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);
            }
        }
    }

    private void SaveBindings()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.MainData != null && InputManager.Instance != null)
        {
            // Bây giờ hàm này sẽ lấy đúng chuỗi JSON chứa phím mới được lưu trong bản sao
            SaveManager.Instance.MainData.settings.keyBindings = InputManager.Instance.GetBindingOverridesJson();
            SaveManager.Instance.SaveToDisk();
        }
    }
}