using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TaleStoneManager : Singleton<TaleStoneManager>
{
    [Header("Tale Stone UI")]
    [SerializeField] private GameObject _taleStonePanel;
    [SerializeField] private TMP_Text _lineText;
    [SerializeField] private TMP_Text _numberText;
    [SerializeField] private float _numberTextFloatingDuration = 0.5f;
    [SerializeField] private float _numberTextFloatingOffset = 2f;
    private Vector2 _originalNumberTextTransform;

    [Header("Dialogue Settings")]
    [SerializeField] private TaleStoneData _currTaleStoneData;
    private int _currTaleStoneIndex;
    private string _currTaleStoneText;
    private bool _isTyping;
    public bool IsTaleStoneActive { get; private set; }

    private Action _onTaleStoneFinishedCallback;

    private void Start()
    {
        _originalNumberTextTransform = _numberText.transform.position;
        EndDialogue(true);
    }

    public void StartTale(TaleStoneData data, Transform npcTransform = null, Action onDialogueFinished = null)
    {
        _numberText.transform.DOMoveY(_originalNumberTextTransform.y + _numberTextFloatingOffset, _numberTextFloatingDuration)
            .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.OutSine);
        if (data == null)
        {
            Debug.LogError("DialogueData is null!");
            return;
        }

        _onTaleStoneFinishedCallback = onDialogueFinished;

        _currTaleStoneData = data;
        _currTaleStoneIndex = 0;
        
        InputManager.Instance.DisableControl(); 

        if (npcTransform != null && CameraManager.Instance != null)
        {
            CameraManager.Instance.ZoomIntoDialogue(npcTransform, () => 
            {
                IsTaleStoneActive = true;
                DisplayCurrentLine();
                ShowDialogue(true);
            });
        }
        else
        {
            IsTaleStoneActive = true;
            DisplayCurrentLine();
            ShowDialogue(true);
        }        
    }

    private void DisplayCurrentLine()
    {
        StopAllCoroutines();

        _currTaleStoneText = _currTaleStoneData.DialogueLines[_currTaleStoneIndex].GetText();
        StartCoroutine(TypeLine());        
    }

    private IEnumerator TypeLine()
    {
        _isTyping = true;
        _lineText.text = "";

        foreach (char letter in _currTaleStoneText)
        {
            _lineText.text += letter;
            yield return new WaitForSecondsRealtime(_currTaleStoneData.TypingSpeed);
        }

        _isTyping = false;

        if (_currTaleStoneData.DialogueLines.Length > _currTaleStoneIndex && _currTaleStoneData.DialogueLines[_currTaleStoneIndex].AutoSkip)
        {
            yield return new WaitForSecondsRealtime(_currTaleStoneData.AutoProgressDelay);
            NextLine();
        }
    }

    public void NextLine()
    {
        if (_currTaleStoneData == null) return;
        
        if (_isTyping)
        {
            StopAllCoroutines();
            _lineText.text = _currTaleStoneText;
            _isTyping = false;
        }
        else if (++_currTaleStoneIndex < _currTaleStoneData.DialogueLines.Length)    
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
        IsTaleStoneActive = false;
        _taleStonePanel.SetActive(false); 
        ShowDialogue(false);

        _lineText.text = "";
        _currTaleStoneIndex = 0;
        _currTaleStoneText = "";
        _currTaleStoneData = null;

        if (!instant && CameraManager.Instance != null)
        {
            CameraManager.Instance.ResetDialogueCamera(() => 
            {
                InputManager.Instance.EnableControl();
                _onTaleStoneFinishedCallback?.Invoke();
                _onTaleStoneFinishedCallback = null;
            });
        }
        else
        {
            if(InputManager.Instance != null)
            {
                InputManager.Instance.EnableControl();
            }
            _onTaleStoneFinishedCallback?.Invoke();
            _onTaleStoneFinishedCallback = null;
        }
    }

    public void ShowDialogue(bool on)
    {
        _taleStonePanel.SetActive(on);
    }

    void Update()
    {
        if (!IsTaleStoneActive) return;

        bool click = InputManager.Instance.Inputs.Interaction.Interact.WasPressedThisFrame();

        if (click)
        {
            NextLine();
        }
    }
}
