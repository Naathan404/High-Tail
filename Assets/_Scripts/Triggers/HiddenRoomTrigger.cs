using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HiddenRoomTrigger : MonoBehaviour
{
    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private List<ObjectToFade> _objectsToFade = new List<ObjectToFade>();
    [SerializeField] private float _tilemapOpacity = 0.5f;
    [SerializeField] private float _fadingDuration = 4f;

    private void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            if(_tilemap != null)
            {
                DOTween.To
                (
                    () => _tilemap.color,
                    x => _tilemap.color = x,
                    new Color(1f, 1f, 1f, _tilemapOpacity),
                    _fadingDuration
                );
            }

            foreach(var obj in _objectsToFade)
            {
                obj.spriteRenderer.DOFade(obj.alpha, _fadingDuration);
            }
        }
    }

    [System.Serializable]
    class ObjectToFade
    {
        public SpriteRenderer spriteRenderer;
        public float alpha = 0.5f;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            if(_tilemap != null)
            {
                DOTween.To
                (
                    () => _tilemap.color,
                    x => _tilemap.color = x,
                    new Color(1f, 1f, 1f, 1f),
                    _fadingDuration
                );
            }

            foreach(var obj in _objectsToFade)
            {
                obj.spriteRenderer.DOFade(1f, _fadingDuration);
            }
        }
    }
}
