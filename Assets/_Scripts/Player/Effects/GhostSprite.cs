using DG.Tweening;
using UnityEngine;

public class GhostSprite : MonoBehaviour
{
    private SpriteRenderer _sr;
    private Color _color;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _color = new Color(0f, 0f, 0f, 1f);
    }

    public void Init(Sprite sprite, Vector3 position, Vector3 scale, Color color, float duration)
    {
        transform.position = position;
        transform.localScale = scale;
        _sr.sprite = sprite;
        _sr.color = color;

        _sr.DOKill();
        _sr.DOFade(0, duration).OnComplete(() => gameObject.SetActive(false));
    }
}
