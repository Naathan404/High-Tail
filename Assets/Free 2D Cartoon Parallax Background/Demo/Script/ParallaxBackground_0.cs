using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground_0 : MonoBehaviour
{
    public bool Camera_Move;
    public float Camera_MoveSpeed = 1.5f;
    [Header("Layer Setting")]
    public List<float> Layer_Speed = new List<float>();
    public List<GameObject> Layer_Objects = new List<GameObject>();

    public Transform _camera;
    private List<float> startPos = new List<float>();
    private float boundSizeX;
    private float sizeX;
    private GameObject Layer_0;
void Start()
{
    // Kiểm tra nếu danh sách trống để tránh lỗi dòng tiếp theo
    if (Layer_Objects.Count == 0) return;

    sizeX = Layer_Objects[0].transform.localScale.x;
    boundSizeX = Layer_Objects[0].GetComponent<SpriteRenderer>().sprite.bounds.size.x;
    for (int i = 0; i < Layer_Objects.Count; i++)
    {
        startPos.Add(_camera.position.x);
    }
}

void Update()
{
    if (Camera_Move)
    {
        _camera.position += Vector3.right * Time.deltaTime * Camera_MoveSpeed;
    }
    for (int i = 0; i < Layer_Objects.Count; i++)
    {
        if (i >= Layer_Speed.Count) break;

        float temp = (_camera.position.x * (1 - Layer_Speed[i]));
        float distance = _camera.position.x * Layer_Speed[i];
        
        Layer_Objects[i].transform.position = new Vector2(startPos[i] + distance, _camera.position.y);

        float offset = boundSizeX * sizeX;
        if (temp > startPos[i] + offset)
        {
            startPos[i] += offset;
        }
        else if (temp < startPos[i] - offset)
        {
            startPos[i] -= offset;
        }
    }
}
}
