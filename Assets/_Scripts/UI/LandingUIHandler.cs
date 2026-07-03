using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class LandingScene : MonoBehaviour
{
    [SerializeField] CanvasGroup _pressAnyKeyTextCanvasGroup;
    [SerializeField] private float _fadeAmount = 0.5f;
    [SerializeField] private float _fadeDuration = 0.5f;

    private void Start()
    {
        _pressAnyKeyTextCanvasGroup.DOFade(_fadeAmount, _fadeDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
        

        AudioManager.Instance.PlayMusic(SoundName.Menu);
    }

    private void Update()
    {
        bool isKeyPressed = UnityEngine.InputSystem.Keyboard.current != null && 
                            UnityEngine.InputSystem.Keyboard.current.anyKey.wasPressedThisFrame;
        bool isMousePressed = UnityEngine.InputSystem.Mouse.current != null &&
                            UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame;

        if (isKeyPressed || isMousePressed)
        {
            this.enabled = false; 
            
            AudioManager.Instance.PlaySFX(SoundName.UI_Click_Keycap);
            Debug.Log("Chuyển scene an toàn!");
            SceneTransitionHandler.Instance.LoadSceneAsync("MainMenu", "_CoreScene", "LandingScene");
        }
    }
}
