using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class GeneralSetting : Singleton<GeneralSetting>
{
    public Action<Language> OnLanguageChanged;

    public enum Language { Vietnamese, English };

    [Header("Settings State")]
    public Language currentLanguage = Language.Vietnamese;
    public bool autoSave = true;
    public string MainCharacterName = "";

    private void Start()
    {
        SyncFromSaveData();
    }

    #region Data Sync Logic
    public void SyncFromSaveData()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.MainData == null) return;

        var settingsData = SaveManager.Instance.MainData.settings;
        if (settingsData == null) return;

        // Đồng bộ TẤT CẢ các biến từ Data vào Setting
        currentLanguage = (Language)settingsData.languageIndex;
        autoSave = settingsData.autoSave;
        MainCharacterName = settingsData.mainCharacterName;

        // Kích hoạt logic UI (Ngôn ngữ)
        string targetLocaleCode = (currentLanguage == Language.Vietnamese) ? "vi" : "en";
        StartCoroutine(UpdateUnityLocalization(targetLocaleCode));
    }

    private void SaveSettings()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.MainData == null) return;

        var settingsData = SaveManager.Instance.MainData.settings;
        if (settingsData == null) return;

        settingsData.languageIndex = (int)currentLanguage;
        settingsData.autoSave = autoSave;
        settingsData.mainCharacterName = MainCharacterName;

        SaveManager.Instance.SaveToDisk();
    }
    #endregion

    #region PUBLIC API
    public void ChangeLanguage(Language language)
    {
        if (currentLanguage == language) return;

        currentLanguage = language;

        // Gọi hàm lưu tập trung
        SaveSettings();

        string targetLocaleCode = (language == Language.Vietnamese) ? "vi" : "en";
        StartCoroutine(UpdateUnityLocalization(targetLocaleCode));
    }

    public void ChangeAutoSave(bool isOn)
    {
        if (autoSave == isOn) return;

        autoSave = isOn;
        SaveSettings(); 
    }

    public void ChangeMainCharacterName(string newName)
    {
        if (MainCharacterName == newName) return;

        MainCharacterName = newName;
        SaveSettings();
    }
    #endregion

    private IEnumerator UpdateUnityLocalization(string localeCode)
    {
        yield return LocalizationSettings.InitializationOperation;

        var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
            OnLanguageChanged?.Invoke(currentLanguage);
        }
        else
        {
            Debug.LogError($"[Localization] Không tìm thấy ngôn ngữ có mã: {localeCode}");
        }
    }
}

[System.Serializable]
public struct Text
{
    public string textVI;
    public string textEN;

    public string GetText()
    {
        switch (GeneralSetting.Instance.currentLanguage)
        {
            case GeneralSetting.Language.English:
                return textEN;
            default:
                return textVI;
        }
    }

    public int GetLength()
    {
        switch (GeneralSetting.Instance.currentLanguage)
        {
            case GeneralSetting.Language.English:
                return textEN.Length;
            default:
                return textVI.Length;
        }
    }
}