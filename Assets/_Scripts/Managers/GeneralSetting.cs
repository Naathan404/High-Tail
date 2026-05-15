using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class GeneralSetting : Singleton<GeneralSetting>
{
    public Action<Language> OnLanguageChanged;
    public bool autoSave = true;
    public string MainCharacterName;

    public enum Language { Vietnamese, English };

    private void Start()
    {
            StartCoroutine(UpdateUnityLocalization(currentLanguage == Language.Vietnamese ? "vi" : "en"));
    }

    public Language currentLanguage = Language.Vietnamese;

    public void ChangeLanguage(Language language)
    {
        if (currentLanguage == language) return;

        currentLanguage = language;
        string targetLocaleCode = "";

        if (language == Language.Vietnamese)
        {
            targetLocaleCode = "vi";
        }
        else
        {
            targetLocaleCode = "en";
        }

        StartCoroutine(UpdateUnityLocalization(targetLocaleCode));
    }

    private IEnumerator UpdateUnityLocalization(string localeCode)
    {
        yield return LocalizationSettings.InitializationOperation;

        var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
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
        switch(GeneralSetting.Instance.currentLanguage)
        {
            case GeneralSetting.Language.English:
                return textEN;
            default:
                return textVI;
        }
    }

    public int GetLength()
    {
        switch(GeneralSetting.Instance.currentLanguage)
        {
            case GeneralSetting.Language.English:
                return textEN.Length;
            default:
                return textVI.Length;
        }        
    }
}
