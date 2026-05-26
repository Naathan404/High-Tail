using UnityEngine;

public class BiomeTrigger : MonoBehaviour
{
    [System.Serializable]
    public struct Name
    {
        [SerializeField] private string _enName;
        [SerializeField] private string _vnName;

        // Functions
        public string GetName()
        {
            if (GeneralSetting.Instance.currentLanguage == GeneralSetting.Language.Vietnamese)
            {
                return _vnName;
            }
            else
            {
                return _enName;
            }
        }
    }

    [SerializeField] private Name _biomeName;
    [SerializeField] private Trigger _mainTrigger;
    [SerializeField] private Trigger _resetTrigger;
    private bool _hasBeenTriggered = true;
    private bool _hasBeenReset = false;

    private void Awake()
    {
        _hasBeenTriggered = true;
        _hasBeenReset = false;
    }

    private void OnEnable()
    {
        _mainTrigger.OnStepIn += Notify;
        _resetTrigger.OnStepIn += ResetTrigger;
    }

    private void OnDisable()
    {
        _mainTrigger.OnStepIn -= Notify;
        _resetTrigger.OnStepIn -= ResetTrigger;
    }

    private void Notify()
    {
        if (_hasBeenTriggered)
        {
            return;
        }
        _hasBeenTriggered = true;
        BiomeNotifier.Instance.ShowBiomeNotification(_biomeName.GetName());
    }

    private void ResetTrigger()
    {
        if (_hasBeenReset) return;
        _hasBeenTriggered = false;
        _hasBeenReset = true;
    }
}
