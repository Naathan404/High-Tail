using UnityEngine;

[CreateAssetMenu(fileName = "New TaleStone Data", menuName = "High Tail/TaleStoneData")]
public class TaleStoneData : ScriptableObject
{
    public DialogueLine[] DialogueLines;
    public float AutoProgressDelay = 1.5f;
    public float TypingSpeed = 0.05f;

    public bool IsActivated = false;
    
    [System.Serializable]
    public struct DialogueLine
    {
        [Header("Content")]
        [TextArea(3, 5)] public string textVI; // Tiếng Việt
        [TextArea(3, 5)] public string textEN; // Tiếng Anh
        public bool AutoSkip;

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
