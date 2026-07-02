using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class TaleStoneTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    private Collider2D _trigger;
    [SerializeField] private GameObject _interactMark;
    [SerializeField] private TaleStoneData _taleStoneData;
    [SerializeField] private Transform _camCenterTransform;
    [SerializeField] private TaleStoneType _type;
    [SerializeField] private GameObject _light;

    [SerializeField] private int _skillIndex = -1; 

    private SpriteRenderer _spriteRenderer;
    private Material _allIn1Material;
    private readonly string _shaderProperty = "_OutlineAlpha";

    [Header("Camera Collider")]
    [SerializeField] private BoxCollider2D _confiderCollider;

    [Header("Event Callback")]
    public UnityEvent OnTaleStoneDialogueCompleted;

    [Header("Decorations")]
    [SerializeField] private List<Transform> _decorations = new List<Transform>();

    [Header("Skill Unlock Tweens (New)")]
    [SerializeField] private Animator _animator;             
    [SerializeField] private float _elevateYOffset = 1.5f;     
    [SerializeField] private float _elevateDuration = 2.5f;    
    [SerializeField] private float _stoneFloatOffset = 0.2f; 
    [SerializeField] private float _stoneFloatDuration = 1.8f;
    PlayerController _player;
    private bool _canInteract = false;
    private AudioSource _audioSource;
    private float _originalY;                              
    private Tweener _stoneFloatTween;

    public static event Action<Transform> OnTaleStoneActivated;

    private void Awake()
    {
        _trigger = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
        _allIn1Material = _spriteRenderer.material;
        _allIn1Material.SetFloat(_shaderProperty, 0f);

        _originalY = transform.position.y;

        _player = FindAnyObjectByType<PlayerController>();
        
        if (_type == TaleStoneType.SkillUnlock)
        {
            StartDecorationsFloating();

            // nếu đã mở khóa skill ở tale stone này rồi
            if ((_taleStoneData != null && _taleStoneData.IsActivated) || (_player != null && _player.PlayerSkillUnlockStatus[_skillIndex]))
            {
                transform.position = new Vector3(transform.position.x, _originalY + _elevateYOffset, transform.position.z);
                if (_animator != null) _animator.Play("Activated");
                StartStoneFloating(); 
            }
            else
            {
                if (_animator != null) _animator.Play("Idle");
            }
        }

        _interactMark.transform.DOMoveY(_interactMark.transform.position.y + _stoneFloatOffset, 0.25f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    [System.Obsolete]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            if((_type == TaleStoneType.SkillUnlock && _taleStoneData.IsActivated) || (_player != null && _player.PlayerSkillUnlockStatus[_skillIndex])) return;
            _canInteract = true;
            _allIn1Material.SetFloat(_shaderProperty, 1f);
            collision.TryGetComponent<PlayerController>(out _player);
        }
    }

    [System.Obsolete]
    private void Update()
    {
        _interactMark.SetActive(_canInteract);
        if(_canInteract == false) return;
        if(_type == TaleStoneType.SkillUnlock && (!_canInteract || _taleStoneData.IsActivated )) return;
        // nếu _canInteract và isActivated = false
        if(InputManager.Instance.Inputs.Interaction.Interact.WasPressedThisFrame())
        {
            _player.Rb.linearVelocity = Vector2.zero;
            CameraManager.Instance.SwitchRoom(_confiderCollider, 50, false, 80f, true);
            _canInteract = false;
            PlaySound();

            if (_type == TaleStoneType.SkillUnlock)
            {
                ExecuteSkillUnlockSequence();
            }

            TaleStoneManager.Instance.StartTale(
            _taleStoneData,
            _camCenterTransform,
            () =>
            {
                OnTaleStoneDialogueCompleted?.Invoke();
                if(_type == TaleStoneType.SkillUnlock)
                {
                    AudioManager.Instance.PlaySFX(SoundName.Reward);
                    _canInteract = false;
                }
                else
                {
                    _canInteract = true;
                }
            });
            _taleStoneData.IsActivated = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            AudioManager.Instance.FadeOutAndStop(_audioSource);
            _interactMark.SetActive(false);
            _canInteract = false;
            _allIn1Material.SetFloat(_shaderProperty, 0f);
        }
    }

    #region Skill Unlock Visual Logic
    private void StartDecorationsFloating()
    {
        foreach (var decor in _decorations)
        {
            if (decor == null) continue;

            float randomOffset = UnityEngine.Random.Range(0.15f, 0.35f);
            float randomDuration = UnityEngine.Random.Range(1.2f, 2f);

            decor.DOMoveY(decor.position.y + randomOffset, randomDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetDelay(UnityEngine.Random.Range(0f, 0.5f));
        }
    }

    private void ExecuteSkillUnlockSequence()
    {
        if (_animator != null) _animator.Play("Activated");
        CameraShakeManager.Instance.ShakeCustom(0.5f);
        //OnTaleStoneActivated?.Invoke(transform);
        transform.DOMoveY(_originalY + _elevateYOffset, _elevateDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                StartStoneFloating();
            });
    }

    private void StartStoneFloating()
    {
        if (_stoneFloatTween != null) _stoneFloatTween.Kill();
        float targetY = _originalY + _elevateYOffset;
        _stoneFloatTween = transform.DOMoveY(targetY + _stoneFloatOffset, _stoneFloatDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
    #endregion

    private void PlaySound()
    {
        GameObject tempGO = new GameObject("TempAudio"); // create the temp object
        tempGO.transform.position = this.transform.position; 
        _audioSource = tempGO.AddComponent<AudioSource>();

        AudioManager.Instance.Play3DSound(SoundName.TaleStone, _audioSource);
    }

    public enum TaleStoneType
    {
        Normal,
        Scratched,
        SkillUnlock
    }
}