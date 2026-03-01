using System;
using UnityEngine;

public class GeneralSetting : MonoBehaviour
{
    public static GeneralSetting Instance;
    public Action<Language> OnLanguageChanged;

    public enum Language { Vietnamese, English };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Language currentLanguage = Language.Vietnamese;

    public void ChangeLanguage()
    {
        if (currentLanguage == Language.Vietnamese)
        {
            currentLanguage = Language.English;
        }
        else
        {
            currentLanguage = Language.Vietnamese;
        }
    }
}
