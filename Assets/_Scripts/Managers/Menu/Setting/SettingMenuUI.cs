using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingMenuUI : MonoBehaviour
{
    [Header("Language")]
    [SerializeField] private GameObject _languagePanel;
    [SerializeField] private Button _viButton;
    [SerializeField] private Button _enButton;

    [Header("Volume")]
    [SerializeField] private Slider _masterVolumeSlider;
    [SerializeField] private Slider _bgmVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;

    private void Start()
    {
        if (_viButton != null) _viButton.onClick.AddListener(() => ChangeLanguage(GeneralSetting.Language.Vietnamese));
        if (_enButton != null) _enButton.onClick.AddListener(() => ChangeLanguage(GeneralSetting.Language.English));

        if (_masterVolumeSlider != null)
            _masterVolumeSlider.onValueChanged.AddListener(val => GeneralSetting.Instance.ChangeMasterVolume(val));

        if (_bgmVolumeSlider != null)
            _bgmVolumeSlider.onValueChanged.AddListener(val => GeneralSetting.Instance.ChangeBGMVolume(val));

        if (_sfxVolumeSlider != null)
            _sfxVolumeSlider.onValueChanged.AddListener(val => GeneralSetting.Instance.ChangeSFXVolume(val));
        SetupPointerUpSave(_masterVolumeSlider);
        SetupPointerUpSave(_bgmVolumeSlider);
        SetupPointerUpSave(_sfxVolumeSlider);
    }

    private void OnEnable()
    {
        SyncUIWithData();
    }

    private void SyncUIWithData()
    {
        if (GeneralSetting.Instance == null) return;

        // Tạm thời tắt lắng nghe sự kiện để tránh việc gán value này kích hoạt lệnh SaveSettings() lưu nhầm
        if (_masterVolumeSlider != null) _masterVolumeSlider.SetValueWithoutNotify(GeneralSetting.Instance.masterVolume);
        if (_bgmVolumeSlider != null) _bgmVolumeSlider.SetValueWithoutNotify(GeneralSetting.Instance.bgmVolume);
        if (_sfxVolumeSlider != null) _sfxVolumeSlider.SetValueWithoutNotify(GeneralSetting.Instance.sfxVolume);
    }

    #region Language Logic
    private void ChangeLanguage(GeneralSetting.Language language)
    {
        GeneralSetting.Instance.ChangeLanguage(language);
    }
    #endregion

    #region Volume Logic
    private void SetupPointerUpSave(Slider slider)
    {
        if (slider == null) return;

        // Lấy EventTrigger hiện có, nếu chưa có thì add vào
        EventTrigger trigger = slider.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = slider.gameObject.AddComponent<EventTrigger>();
        }

        // Tạo một Entry mới cho sự kiện PointerUp
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;

        // Khi xảy ra PointerUp -> Gọi hàm SaveSettings của GeneralSetting
        entry.callback.AddListener((data) => {
            GeneralSetting.Instance.SaveSettings();
        });

        trigger.triggers.Add(entry);
    }
    #endregion
}
