//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.InputSystem;
//using UnityEngine.UI;
//using UnityEngine.InputSystem;

//public class InputSettingMenuUI : MonoBehaviour
//{
//    [Header("Buttons")]
//    [SerializeField] private Button _resetAllButton;

//    [Header("1. Core Actions (Luôn hiển thị)")]
//    // Kéo các prefab RebindUI của Move, Jump, Interact, Menu... vào đây
//    [SerializeField] private RebindActionUI[] _coreRebindActions;

//    [Header("2. Unlockable Actions (Hiện theo Skill)")]
//    // Kéo nguyên cái GameObject (chứa script RebindActionUI) của các kỹ năng vào đây
//    [SerializeField] private GameObject _dashRebindObj;
//    [SerializeField] private GameObject _glideRebindObj;
//    [SerializeField] private GameObject _grabRebindObj;
//    [SerializeField] private GameObject _lightRebindObj;

//    // Danh sách nội bộ để quản lý những nút ĐANG ĐƯỢC HIỆN
//    private List<RebindActionUI> _activeRebindUIs = new List<RebindActionUI>();

//    private void Start()
//    {
//        if (_resetAllButton != null)
//        {
//            _resetAllButton.onClick.AddListener(ResetAllBindings);
//        }
//    }

//    private void OnEnable()
//    {
//        // Mỗi lần mở Menu lên -> Check Save File -> Cập nhật ẩn/hiện nút -> Refresh chữ
//        UpdateActionVisibility();
//        RefreshAllUI();
//    }

//    /// <summary>
//    /// Check dữ liệu Save để quyết định bật/tắt các dòng đổi phím
//    /// </summary>
//    private void UpdateActionVisibility()
//    {
//        _activeRebindUIs.Clear();

//        // 1. Nhóm Core: Luôn bật
//        foreach (var core in _coreRebindActions)
//        {
//            if (core != null)
//            {
//                core.gameObject.SetActive(true);
//                _activeRebindUIs.Add(core);
//            }
//        }

//        // 2. Nhóm Unlockable: Check file save
//        if (SaveManager.Instance != null && SaveManager.Instance.MainData != null)
//        {
//            // Lấy Slot đang chơi hiện tại
//            SaveSlot activeSlot = SaveManager.Instance.GetActiveSlot();

//            if (activeSlot != null && activeSlot.unlockedSkills != null)
//            {
//                SkillSaveData skills = activeSlot.unlockedSkills;

//                // Tắt hoặc Bật GameObject dựa vào biến bool trong SkillSaveData
//                // (Tên biến đằng sau skills.*** ông tự chỉnh lại cho khớp với file SkillSaveData thực tế nhé)
//                CheckAndAddUnlockableUI(_dashRebindObj, skills.DashUnlocked);
//                CheckAndAddUnlockableUI(_glideRebindObj, skills.AirGlideUnlocked);
//                CheckAndAddUnlockableUI(_grabRebindObj, skills.WallSlideUnlocked); // Wall grab/slide
//                CheckAndAddUnlockableUI(_lightRebindObj, skills.GlowUnlocked);     // Nút Light
//            }
//            else
//            {
//                // TRƯỜNG HỢP Ở MAIN MENU (Chưa chọn save): Tạm thời giấu hết kỹ năng ẩn
//                HideAllUnlockables();
//            }
//        }
//        else
//        {
//            HideAllUnlockables();
//        }

//        // 3. Đăng ký sự kiện lưu JSON tự động cho những nút ĐANG HIỆN
//        foreach (var rebindUI in _activeRebindUIs)
//        {
//            // Xóa lắng nghe cũ để tránh bị add đè nhiều lần mỗi khi tắt/mở menu
//            rebindUI.onRebindStop.RemoveAllListeners();
//            rebindUI.onRebindStop.AddListener(OnBindingChanged);
//        }
//    }

//    private void CheckAndAddUnlockableUI(GameObject rebindObj, bool isUnlocked)
//    {
//        if (rebindObj != null)
//        {
//            rebindObj.SetActive(isUnlocked); // Bật/tắt nguyên cái hàng đổi phím

//            if (isUnlocked)
//            {
//                // Nếu được bật, ném nó vào danh sách quản lý chung
//                var rebindUI = rebindObj.GetComponent<RebindActionUI>();
//                if (rebindUI != null) _activeRebindUIs.Add(rebindUI);
//            }
//        }
//    }

//    private void HideAllUnlockables()
//    {
//        if (_dashRebindObj != null) _dashRebindObj.SetActive(false);
//        if (_glideRebindObj != null) _glideRebindObj.SetActive(false);
//        if (_grabRebindObj != null) _grabRebindObj.SetActive(false);
//        if (_lightRebindObj != null) _lightRebindObj.SetActive(false);
//    }

//    public void RefreshAllUI()
//    {
//        foreach (var rebindUI in _activeRebindUIs)
//        {
//            rebindUI.UpdateDisplay();
//        }
//    }

//    private void OnBindingChanged()
//    {
//        if (InputManager.Instance != null)
//        {
//            InputManager.Instance.SaveBindingOverrides();
//        }
//    }

//    private void ResetAllBindings()
//    {
//        if (InputManager.Instance == null || InputManager.Instance.Inputs == null) return;

//        InputManager.Instance.Inputs.asset.RemoveAllBindingOverrides();
//        InputManager.Instance.SaveBindingOverrides();

//        RefreshAllUI();
//    }
//}