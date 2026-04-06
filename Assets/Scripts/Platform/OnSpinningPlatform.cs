using DG.Tweening;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class OnSpinningPlatform : MonoBehaviour
{
    // Se co anchor va platform xoay xung quanh anchor do
    // Co the co 1 hoac nhieu platform, platform nhieu hinh dang
    // Co the chinh toc do xoay cua platform (vong/s), ban kinh tu tam platform den anchor
    // Deadline Tuan 6/4 (han che cuoi tuan)


    [Header("On spinning platform settings")]
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private bool _autoRotate = false;

    private Vector3 _anchor;

    private void Awake()
    {
        _anchor = transform.Find("Anchor").position;
    }

    private void Update()
    {
        
    }

    private void Rotate()
    {

    }
}
