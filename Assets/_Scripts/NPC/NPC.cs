using System.Collections;
using DG.Tweening;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public string NPCID { get; private set; }
    public DialogueData[] dialogueData;
    public GameObject interactIcon;
    public int dialogueIndex = 0;
    [SerializeField] private Transform _visual;

    [Header("Squash & Stretch Settings")]
    [SerializeField] float _animationSpeed = 1.2f;
    [SerializeField] float _squashAmount = 0.06f;
    // [SerializeField] Transform _visual;

    PlayerController player;
    private void Start()
    {
        player = FindAnyObjectByType<PlayerController>();

        // if(_visual != null)
        // {
        //     _visual.transform.DOScaleY(_visual.transform.localScale.y - _squashAmount / 4f,_animationSpeed)
        //         .SetLoops(-1, LoopType.Yoyo)
        //         .SetEase(Ease.InOutSine);

        //     _visual.transform.DOScaleX(_visual.transform.localScale.x + _squashAmount,_animationSpeed)
        //         .SetLoops(-1, LoopType.Yoyo)
        //         .SetEase(Ease.InOutSine);      
        // }
    }

    private void FixedUpdate()
    {
        if (player != null)
        {
            if(player.transform.position.x < transform.position.x)
            {
                _visual.transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                _visual.transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }
}
