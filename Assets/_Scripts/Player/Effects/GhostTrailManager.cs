using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GhostTrailManager : MonoBehaviour
{
    public static GhostTrailManager Instance;
    private List<GhostSprite> _ghostPool = new List<GhostSprite>();
    [SerializeField] private int _poolSize = 10;
    [SerializeField] private GhostSprite _ghostPrefab;
    [SerializeField] private float _ghostDuration = 0.5f;
    [SerializeField] private Color _ghostColor = new Color(0f, 0f, 0f, 1f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        for(int i = 0; i < _poolSize; i++)
        {
            GhostSprite obj = Instantiate(_ghostPrefab, this.transform);
            obj.gameObject.SetActive(false);
            _ghostPool.Add(obj);
        }
    }

    public void SpawnGhost(Sprite sprite, Vector3 position, Vector3 scale)
    {
        GhostSprite ghost = _ghostPool.Find(g => !g.gameObject.activeInHierarchy);
        if(ghost == null) return;
        ghost.Init(sprite, position, scale, _ghostColor, _ghostDuration);
        ghost.gameObject.SetActive(true);
    }

}
