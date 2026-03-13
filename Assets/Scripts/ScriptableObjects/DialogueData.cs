using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Scriptable Objects/DialogueData")]
public class DialogueData : ScriptableObject
{
    public Name nameText1;
    public Name nameText2;
    public DialogueLine[] dialogueLines;
    public bool[] endDialogue;
    public float autoProgressDelay = 1.5f;
    public float typingSpeed = 0.05f;
    public AudioClip voiceSound;
    public float voicePitch = 1f;

    [System.Serializable]
    public struct Name
    {
        public string nameVI;
        public string nameEN;
        public string getName()
        {
            if (GeneralSetting.Instance.currentLanguage == GeneralSetting.Language.English)
            {
                return nameEN;
            }
            return nameVI;
        }
    }
    [System.Serializable]
    public struct DialogueLine
    {
        public Sprite image;
        public bool IsChar1;

        [Header("Content")]
        [TextArea(3, 5)] public string textVI; // Tiếng Việt
        [TextArea(3, 5)] public string textEN; // Tiếng Anh

        public bool autoSkip;

        public string GetText()
        {
            if (GeneralSetting.Instance.currentLanguage == GeneralSetting.Language.English)
            {
                return textEN;
            }
            return textVI;
        }
    }
}
