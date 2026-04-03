using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum VineType
{
    StiffClimb,
    LooseSwing
}
public class Vine : MonoBehaviour
{
    // type of the vine, 1 - player can climb on it up and down, 2 - player can swing left and right

    [Header("Vine Anchors")]
    [SerializeField] private Transform _pointA;
    [SerializeField] private Transform _pointB;

    [Header("Vine Settings")]
    [SerializeField] private VineType _type = VineType.LooseSwing;
    [SerializeField] private int _numberOfSegments;
    [SerializeField] private GameObject _vineSegPrefab;
    [SerializeField] private List<Sprite> _vineSprites = new List<Sprite>();

    // list of vine segments
    private List<GameObject> _segments = new List<GameObject>();

    private void Start()
    {
        GenerateVine();
        // float disX = (_pointB.position.x - _pointA.position.x) / _numberOfSegments;
        // float disY = (_pointB.position.y - _pointA.position.y) / _numberOfSegments;

        // for(int i = 0; i < _numberOfSegments; i++)
        // {
        //     GameObject vineSeg = Instantiate(_vineSegPrefab, this.transform);
        //     vineSeg.GetComponent<SpriteRenderer>()!.sprite = _vineSprites[Random.Range(0, _vineSprites.Count)];
        //     vineSeg.GetComponent<HingeJoint2D>().autoConfigureConnectedAnchor = true;
            
        //     if(i == 0)
        //     {
        //         vineSeg.GetComponent<HingeJoint2D>().connectedBody = _pointA.GetComponent<Rigidbody2D>();
        //         vineSeg.transform.position = _pointA.position + new Vector3(disX * (i), disY * (i));
        //     }
        //     if(i > 0)
        //     {
        //         vineSeg.GetComponent<HingeJoint2D>().connectedBody = _segments[i - 1].GetComponent<Rigidbody2D>();
        //         vineSeg.transform.position = _segments[i - 1].transform.position + new Vector3(disX * (i), disY * (i));
        //     }
        //     _segments.Add(vineSeg);
        // }
    }

    private void GenerateVine()
    {
        Vector2 startPos = _pointA.position;
        Vector2 endPos = _pointB.position;
        
        float stepX = (endPos.x - startPos.x) / _numberOfSegments;
        float stepY = (endPos.y - startPos.y) / _numberOfSegments;

        Rigidbody2D previousRb = _pointA.GetComponent<Rigidbody2D>();
        
        if (previousRb == null)
        {
            Debug.LogError("Điểm neo thiếu rigidbody2d kìa cậu! thêm cmn vào đi");
            return;
        }

        for (int i = 0; i < _numberOfSegments; i++)
        {
            GameObject vineSeg = Instantiate(_vineSegPrefab, transform);
            if (_vineSprites.Count > 0)
            {
                vineSeg.GetComponent<SpriteRenderer>().sprite = _vineSprites[Random.Range(0, _vineSprites.Count)];
            }

            vineSeg.transform.position = startPos + new Vector2(stepX * (i), stepY * (i));
            vineSeg.GetComponent<VineSegment>().Type = _type;
            HingeJoint2D joint = vineSeg.GetComponent<HingeJoint2D>();
            if (joint != null)
            {
                joint.connectedBody = previousRb;
                joint.autoConfigureConnectedAnchor = true;

                joint.enableCollision = false; 
                ApplyPhysicsBasedOnType(vineSeg.GetComponent<Rigidbody2D>(), joint);
            }
            if(i == 0)
                joint.autoConfigureConnectedAnchor = false;

            _segments.Add(vineSeg);
            
            previousRb = vineSeg.GetComponent<Rigidbody2D>();
        }
    }

    private void ApplyPhysicsBasedOnType(Rigidbody2D rb, HingeJoint2D joint)
    {
        if(_type == VineType.StiffClimb)
        {
            JointAngleLimits2D limits = new JointAngleLimits2D { min = -3f, max = 3f};
            joint.limits = limits;
            joint.useLimits = true;

            rb.mass = 2f;
            rb.angularDamping = 5f;
        }
        else if (_type == VineType.LooseSwing)
        {
            JointAngleLimits2D limits = new JointAngleLimits2D { min = -75f, max = 75f };
            joint.limits = limits;
            joint.useLimits = true;

            rb.angularDamping = 0.5f;
            rb.mass = 0.7f;
        }
    }

    private void OnDrawGizmos()
    {
        if(_type == VineType.StiffClimb)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.green;
        for(int i = 0; i < _numberOfSegments; i++)
        {
            Gizmos.DrawLine(_pointA.position, _pointB.position);
        }
    }
}
