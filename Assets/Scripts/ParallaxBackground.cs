using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxBackground : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector2 _parallaxMultiplier; // Tốc độ (0-1)
    [SerializeField] private bool _infiniteHorizontal = true;
    [SerializeField] private bool _infiniteVertical = false;

    private Transform _cameraTransform;
    private Vector3 _cameraStartPos;
    private Vector3 _layerStartPos;
    private float _textureUnitSizeX;
    private float _textureUnitSizeY;

    void Start()
    {
        _cameraTransform = Camera.main.transform;
        _cameraStartPos = _cameraTransform.position;
        _layerStartPos = transform.position;

        Sprite sprite = GetComponent<SpriteRenderer>()?.sprite;
        if (sprite == null) return;
        Texture2D texture = sprite.texture;
        if (texture != null)
        {
            _textureUnitSizeX = texture.width / sprite.pixelsPerUnit;
            _textureUnitSizeY = texture.height / sprite.pixelsPerUnit;
        }
    }

    void LateUpdate()
    {
        Vector3 totalCameraMovement = _cameraTransform.position - _cameraStartPos;

        // Infinite scrolling logic
        if (_infiniteHorizontal)
        {
            float relativeDistX = totalCameraMovement.x * (1 - _parallaxMultiplier.x);

            if (Mathf.Abs(relativeDistX - (_layerStartPos.x - _cameraStartPos.x)) >= _textureUnitSizeX)
            {
                // Mathf.Sign: nếu >= 0 return 1, nếu < 0 return -1;
                float offset = Mathf.Sign(relativeDistX - (_layerStartPos.x - _cameraStartPos.x)) * _textureUnitSizeX;
                _layerStartPos.x += offset;
            }
        }

        if (_infiniteVertical)
        {
            float relativeDistY = totalCameraMovement.y * (1 - _parallaxMultiplier.y);
            if (Mathf.Abs(relativeDistY - (_layerStartPos.y - _cameraStartPos.y)) >= _textureUnitSizeY)
            {
                float offset = Mathf.Sign(relativeDistY - (_layerStartPos.y - _cameraStartPos.y)) * _textureUnitSizeY;
                _layerStartPos.y += offset;
            }
        }

        // Update layer position
        float posX = _layerStartPos.x + totalCameraMovement.x * _parallaxMultiplier.x;
        float posY = _layerStartPos.y + totalCameraMovement.y * _parallaxMultiplier.y;
        transform.position = new Vector3(posX, posY, transform.position.z);
    }
}