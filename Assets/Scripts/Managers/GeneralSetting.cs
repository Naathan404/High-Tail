using System;
using UnityEngine;
using UnityEngine.UIElements;

public class GeneralSetting : MonoBehaviour
{
    //---Khi thay đổi general setting thì
    //------1. Vào SaveData thêm biến dữ liệu tương ứng
    //------2. Vào SaveSystem thêm dòng code để save và load dữ liệu đó

    public static GeneralSetting Instance;
    public Action<Language> OnLanguageChanged;
    public bool autoSave = true;
    public bool autoCheckPoint = true;
    public string MainCharacterName;

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
