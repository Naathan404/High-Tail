using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Rendering.MaterialUpgrader;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Image image;
    public TMP_Text text;
    public TMP_Text nameText;
    public GameObject dialoguePanel;

    private bool _isTyping;
    public bool IsDialogueActive { get; private set; }

    public DialogueData curDialogueData;
    private int _curDialogueIndex;
    private string _curDialogueText;


    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    void Start()
    {
        EndDialogue();
    }

    void Update()
    {
        if (!IsDialogueActive) return;

        // Kiểm tra an toàn xem thiết bị có tồn tại không và có được bấm trong frame này không
        bool isLeftClick = InputManager.Instance.Inputs.Interaction.Interact.WasPressedThisFrame();
        //bool isSpacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        if (isLeftClick)
        {
            NextLine();
        }
    }

    public void ShowDialogue(bool on)
    {
        dialoguePanel.SetActive(on);
    }

    public void StartDialogue(DialogueData data)
    {
        if (data == null)
        {
            Debug.LogError("DialogueData is null!");
            return;
        }

        curDialogueData = data;
        _curDialogueIndex = 0;
        IsDialogueActive = true;

        nameText.text = curDialogueData.nameText.getName();
        ShowDialogue(true);
        PauseGameManager.SetPause(true);

        DisplayCurrentLine();
    }

    private void DisplayCurrentLine()
    {
        StopAllCoroutines();

        image.sprite = curDialogueData.dialogueLines[_curDialogueIndex].image;
        _curDialogueText = curDialogueData.dialogueLines[_curDialogueIndex].GetText();

        StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        _isTyping = true;
        text.text = "";

        foreach (char letter in _curDialogueText)
        {
            text.text += letter;

            // Âm thanh gõ (nếu có sound library)
            //Controller_Sound.Play("Type");

            yield return new WaitForSecondsRealtime(curDialogueData.typingSpeed);
        }

        _isTyping = false;

        if (curDialogueData.dialogueLines.Length > _curDialogueIndex && curDialogueData.dialogueLines[_curDialogueIndex].autoSkip)
        {
            yield return new WaitForSecondsRealtime(curDialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void NextLine()
    {
        if (curDialogueData == null)
        {
            return;
        }
        if (_isTyping)
        {
            StopAllCoroutines();
            text.text = _curDialogueText;
            _isTyping = false;
        }
        else if (++_curDialogueIndex < curDialogueData.dialogueLines.Length)    
        {
            DisplayCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        IsDialogueActive = false;
        dialoguePanel.SetActive(false);
        text.text = "";
        if (image != null)
        {
            image.sprite = null;
        }
        _curDialogueIndex = 0;
        _curDialogueText = "";
        curDialogueData = null;

        ShowDialogue(false);
        PauseGameManager.SetPause(false);
    }
}
