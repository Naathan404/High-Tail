using UnityEngine;
using System.Collections;
using DG.Tweening;
using System;

public class FallingPlatform : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform _targetPosition;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _fallDuration = 2.5f;
    [SerializeField] private float _returnDuration = 2.5f;

    private Vector2 _defaultPosition;


    private void Awake()
    {
        _defaultPosition = transform.position;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if(player != null) player.transform.parent = this.transform;
            transform.DOMoveY(_targetPosition.position.y, _fallDuration, false);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if(player != null) player.ReturnToCoreScene();
            transform.DOMoveY(_defaultPosition.y, _returnDuration, false);
        }
    }

    private void ReturnPlatform()
    {
        transform.DOMoveY(_defaultPosition.y, _returnDuration, false);
    }
}
