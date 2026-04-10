using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SpinningPlatform : MonoBehaviour
{
    // Se co anchor va platform xoay xung quanh anchor do
    // Co the co 1 hoac nhieu platform, platform nhieu hinh dang
    // Co the chinh toc do xoay cua platform (vong/s), ban kinh tu tam platform den anchor
    // Deadline Tuan 6/4 (han che cuoi tuan)


    [Header("Spinning platform settings")]
    [SerializeField] private List<GameObject> _platforms;
    [SerializeField] private float _rotationSpeed = 3;
    [SerializeField] private float _radius = 5f;

    private int _platformCount; // Number of all child platforms in spinning platform

    private void Awake()
    {
        _platformCount = _platforms.Count;
    }

    private void Start()
    {
        SetPosition();
    }

    private void Update()
    {
        Rotate();
    }

    private void Rotate()
    {
        float rotationAmount = 360f * _rotationSpeed * Time.deltaTime;
        transform.Rotate(Vector3.forward, rotationAmount);
    }

    private void SetPosition()
    {
        float angleStep = 360 / _platformCount;
        int count = 0;
        foreach (var platform in _platforms)
        {
            Vector3 position = CalculatePosition(angleStep * count);
            if (position != null)
            {
                platform.transform.position = this.transform.position + position;
                count++;
            }
        }
    }

    // Calculate and set the position for every platforms
    private Vector3 CalculatePosition(float angleInDegrees)
    {
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleInRadians) * _radius, Mathf.Sin(angleInRadians) * _radius, 0f);
    }

    private void OnDrawGizmos()
    {
        Handles.color = Color.hotPink;
        Handles.DrawWireDisc(transform.position, Vector3.forward, _radius);
        //Gizmos.color = Color.greenYellow;
        //for (int i = 0; i < _platformCount; i++)
        //{
        //    Gizmos.DrawLine(transform.position, transform.position + CalculatePosition(i * (360 / _platformCount)));
        //}
    }
}
