using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System; // Dùng cho Action

public class DialogueManager : Singleton<DialogueManager>
{
    [Header("Dialogue UI")]
    [SerializeField] private TMP_Text _lineText;
    [SerializeField] private TMP_Text _yourNameText;
    [SerializeField] private TMP_Text _otherNameText;
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private Image _yourImage;
    [SerializeField] private Image _otherImage;

    [Header("Dialogue Settings")]
    [SerializeField] private DialogueData _curDialogueData;
    private int _curDialogueIndex;
    private string _curDialogueText;
    private bool _isTyping;
    public bool IsDialogueActive { get; private set; }

    void Start()
    {
        EndDialogue(true); 
    }

    void Update()
    {
        if (!IsDialogueActive) return;

        bool isLeftClick = InputManager.Instance.Inputs.Interaction.Interact.WasPressedThisFrame();

        if (isLeftClick)
        {
            NextLine();
        }
    }

    // THAY ĐỔI LỚN: Thêm tham số Transform npcTransform
    public void StartDialogue(DialogueData data, Transform npcTransform = null)
    {
        if (data == null)
        {
            Debug.LogError("DialogueData is null!");
            return;
        }

        _curDialogueData = data;
        _curDialogueIndex = 0;
        
        InputManager.Instance.DisableControl(); 

        if (npcTransform != null && CameraManager.Instance != null)
        {
            CameraManager.Instance.ZoomIntoDialogue(npcTransform, () => 
            {
                IsDialogueActive = true;
                _otherImage.sprite = _curDialogueData.CharSprite;
                DisplayCurrentLine();
                ShowDialogue(true);
            });
        }
        else
        {
            IsDialogueActive = true;
            _otherImage.sprite = _curDialogueData.CharSprite;
            DisplayCurrentLine();
            ShowDialogue(true);
        }
    }

    private void DisplayCurrentLine()
    {
        StopAllCoroutines();

        if(_curDialogueData.DialogueLines[_curDialogueIndex].IsYou)
        {
            _yourImage.gameObject.SetActive(true);
            _yourNameText.gameObject.SetActive(true);
            _otherImage.gameObject.SetActive(false);
            _otherNameText.gameObject.SetActive(false);
        }
        else
        {
            _yourImage.gameObject.SetActive(false);
            _yourNameText.gameObject.SetActive(false);
            _otherImage.gameObject.SetActive(true); 
            _otherNameText.gameObject.SetActive(true);
        }
        _curDialogueText = _curDialogueData.DialogueLines[_curDialogueIndex].GetText();
        
        _otherNameText.text = _curDialogueData.DialogueLines[_curDialogueIndex].IsYou ? 
                        GeneralSetting.Instance.MainCharacterName : _curDialogueData.Name.GetText();
        StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        _isTyping = true;
        _lineText.text = "";

        foreach (char letter in _curDialogueText)
        {
            _lineText.text += letter;
            yield return new WaitForSecondsRealtime(_curDialogueData.TypingSpeed);
        }

        _isTyping = false;

        if (_curDialogueData.DialogueLines.Length > _curDialogueIndex && _curDialogueData.DialogueLines[_curDialogueIndex].autoSkip)
        {
            yield return new WaitForSecondsRealtime(_curDialogueData.AutoProgressDelay);
            NextLine();
        }
    }

    public void NextLine()
    {
        if (_curDialogueData == null) return;
        
        if (_isTyping)
        {
            StopAllCoroutines();
            _lineText.text = _curDialogueText;
            _isTyping = false;
        }
        else if (++_curDialogueIndex < _curDialogueData.DialogueLines.Length)    
        {
            DisplayCurrentLine();
        }
        else
        {
            EndDialogue(false);
        }
    }


    public void EndDialogue(bool instant = false)
    {
        StopAllCoroutines();
        IsDialogueActive = false;
        _dialoguePanel.SetActive(false); 
        ShowDialogue(false);

        _lineText.text = "";
        _curDialogueIndex = 0;
        _curDialogueText = "";
        _curDialogueData = null;

        if (!instant && CameraManager.Instance != null)
        {
            CameraManager.Instance.ResetDialogueCamera(() => 
            {
                InputManager.Instance.EnableControl();
            });
        }
        else
        {
            if(InputManager.Instance != null)
            {
                InputManager.Instance.EnableControl();
            }
        }
    }

    public void ShowDialogue(bool on)
    {
        _dialoguePanel.SetActive(on);
    }
}