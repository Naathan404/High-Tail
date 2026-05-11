using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UIElements;

public class SimpleDoor : MonoBehaviour, ILightPulseReactive
{
    [SerializeField] private Vector2 _targetPos;
    [SerializeField] private float _openDuration = 5f;
    [SerializeField] private float _shakeStrenght = 0.5f;
    [SerializeField] private float _waitTime = 1f;
    [SerializeField] private List<GameObject> _objectsToFalse = new List<GameObject>();
    [SerializeField] private bool isHorizontal = false;
    private Vector2 _originalPos;
    private bool _isOpened = false;
    

    private void Start()
    {
        _originalPos = this.transform.position;

        DeathScreenManager.OnDeathScreenTriggered += ResetDoor;
    }
    

    private void OnDestroy()
    {
        DeathScreenManager.OnDeathScreenTriggered -= ResetDoor;        
    }
    public void ReactToLightPulse()
    {
        if(_isOpened) return;
        StartCoroutine(Open());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && isHorizontal)
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if(player != null)
            {
                player.transform.SetParent(this.transform);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && isHorizontal)
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if(player != null)
            {
                player.ReturnToCoreScene();
            }
        }
    }

    private IEnumerator Open()
    {
        yield return new WaitForSeconds(_waitTime);
        transform.DOMove(_originalPos + _targetPos, _openDuration).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                foreach(var obj in _objectsToFalse)
                {
                    obj.SetActive(false);
                }

                _isOpened = true;
            });
        CameraShakeManager.Instance.ShakeCustom(_shakeStrenght);
    }

    private void OpenDoor()
    {
        transform.DOMove(_targetPos, _openDuration).SetEase(Ease.OutBounce);
        CameraShakeManager.Instance.ShakeCustom(_shakeStrenght);
    }

    private void ResetDoor()
    {
        transform.DOKill();
        transform.position = _originalPos;
        foreach(var obj in _objectsToFalse)
        {
            obj.SetActive(true);
        }

        _isOpened = false;
    }
}
