using UnityEngine;
using UnityEngine.UI;

public class SettingMenuUI : MonoBehaviour
{
    [Header("Language")]
    [SerializeField] private GameObject _languagePanel;
    [SerializeField] private Button _viButton;
    [SerializeField] private Button _enButton;

    private void Start()
    {
        if (_viButton != null) _viButton.onClick.AddListener(() => ChangeLanguage(GeneralSetting.Language.Vietnamese));
        if (_enButton != null) _enButton.onClick.AddListener(() => ChangeLanguage(GeneralSetting.Language.English));
    }

    private void ChangeLanguage(GeneralSetting.Language language)
    {
        GeneralSetting.Instance.ChangeLanguage(language);
    }
}
