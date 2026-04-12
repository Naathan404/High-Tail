using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Data", menuName = "High Tail/DialogueData")]
public class DialogueData : ScriptableObject
{
    public Text Name;
    public Sprite CharSprite;
    public DialogueLine[] DialogueLines;
    public bool[] EndDialogue;
    public float AutoProgressDelay = 1.5f;
    public float TypingSpeed = 0.05f;
    public AudioClip VoiceSound;
    public float VoicePitch = 1f;

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
    }
    [System.Serializable]
    public struct DialogueLine
    {
        public bool IsYou;

        [Header("Content")]
        [TextArea(3, 5)] public string textVI; // Tiếng Việt
        [TextArea(3, 5)] public string textEN; // Tiếng Anh

        public bool autoSkip;

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
    }
}
