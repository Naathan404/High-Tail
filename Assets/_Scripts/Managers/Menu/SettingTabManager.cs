using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsTabManager : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public Button tabButton;     
        public GameObject tabContent; 
    }

    [SerializeField] private List<Tab> _tabs;
    
    [Header("Màu sắc báo hiệu")]
    [SerializeField] private Color _activeColor = Color.white;
    [SerializeField] private Color _inactiveColor = Color.gray;

    private void Start()
    {
        for (int i = 0; i < _tabs.Count; i++)
        {
            int index = i; 
            if (_tabs[i].tabButton != null)
            {
                _tabs[i].tabButton.onClick.AddListener(() => SwitchTab(index));
            }
        }
    }

    // ĐÂY LÀ CHÌA KHÓA KẾT NỐI VỚI MENU MANAGER!
    // Hàm này tự động chạy mỗi khi MenuManager.cs gọi lệnh OpenPanel(_settingsPanel, true)
    private void OnEnable()
    {
        if (_tabs != null && _tabs.Count > 0)
        {
            SwitchTab(0);
        }
    }

    public void SwitchTab(int tabIndex)
    {
        AudioManager.Instance.PlaySFX(SoundName.UI_Click_Button);
        for (int i = 0; i < _tabs.Count; i++)
        {
            bool isActive = (i == tabIndex);
            
            // Bật/Tắt Panel nội dung
            if (_tabs[i].tabContent != null)
            {
                _tabs[i].tabContent.SetActive(isActive);
            }

            // Đổi màu Nút
            if (_tabs[i].tabButton != null)
            {
                _tabs[i].tabButton.GetComponent<Image>().color = isActive ? _activeColor : _inactiveColor;
            }
        }
    }
}