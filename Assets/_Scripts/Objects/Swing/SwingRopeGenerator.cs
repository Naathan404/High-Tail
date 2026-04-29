using System.Collections.Generic;
using UnityEngine;

public class SwingRopeGenerator : MonoBehaviour
{
    [Header("Anchors (Điểm trên trần)")]
    public Transform anchorLeft;
    public Transform anchorRight;

    [Header("Platform Points (Điểm trên ván)")]
    public Transform pointLeft;
    public Transform pointRight;

    [Header("Rope Settings (Cài đặt dây)")]
    public GameObject ropeSegmentPrefab;
    public List<Sprite> ropeSprites = new List<Sprite>();

    public float segmentLength = 0.5f;
    public float slackFactor = 1.05f;

    private void Start()
    {
        // Gọi hàm tạo dây cho cả 2 bên
        GenerateRope(anchorLeft, pointLeft);
        GenerateRope(anchorRight, pointRight);
    }

    private void GenerateRope(Transform topAnchor, Transform bottomPoint)
    {
        if (topAnchor == null || bottomPoint == null || ropeSegmentPrefab == null) return;

        Rigidbody2D previousRb = topAnchor.GetComponent<Rigidbody2D>();
        // Lấy platformRb ngay từ đầu
        Rigidbody2D platformRb = bottomPoint.GetComponentInParent<Rigidbody2D>();

        float distance = Vector2.Distance(topAnchor.position, bottomPoint.position) * slackFactor;
        int numberOfSegments = Mathf.CeilToInt(distance / segmentLength);

        float stepX = (bottomPoint.position.x - topAnchor.position.x) / numberOfSegments;
        float stepY = (bottomPoint.position.y - topAnchor.position.y) / numberOfSegments;

        for (int i = 0; i < numberOfSegments; i++)
        {
            GameObject seg = Instantiate(ropeSegmentPrefab);
            seg.transform.SetParent(topAnchor);
            seg.transform.localScale = ropeSegmentPrefab.transform.localScale;

            if (ropeSprites.Count > 0)
            {
                seg.GetComponent<SpriteRenderer>().sprite = ropeSprites[Random.Range(0, ropeSprites.Count)];
            }

            Vector2 newPos = (Vector2)topAnchor.position + new Vector2(stepX * i, stepY * i);
            seg.transform.position = new Vector3(newPos.x, newPos.y, ropeSegmentPrefab.transform.position.z);

            Rigidbody2D rb = seg.GetComponent<Rigidbody2D>();
            // FIX 1: Tăng mass lên một chút (0.2) để khử hiệu ứng "dây thun" khi lực ly tâm quá mạnh
            rb.mass = 0.2f;
            rb.angularDamping = 1f;

            HingeJoint2D joint = seg.GetComponent<HingeJoint2D>();
            if (joint != null)
            {
                joint.connectedBody = previousRb;
                joint.autoConfigureConnectedAnchor = true;
            }

            // FIX 2: BẮT CHẶT ĐỐT CUỐI CÙNG VÀO VÁN
            if (i == numberOfSegments - 1 && platformRb != null)
            {
                HingeJoint2D finalHook = seg.AddComponent<HingeJoint2D>();
                finalHook.connectedBody = platformRb;

                // Tắt auto để chúng ta tự tay "đóng đinh" nó
                finalHook.autoConfigureConnectedAnchor = false;

                // Điểm neo trên chiếc lá (Tâm chiếc lá)
                finalHook.anchor = Vector2.zero;

                // Ép chiếc lá phải dính vào ĐÚNG tọa độ của PointLeft/PointRight trên mặt ván
                finalHook.connectedAnchor = platformRb.transform.InverseTransformPoint(bottomPoint.position);
            }

            previousRb = rb;
        }

        // (Lưu ý: Đoạn code BƯỚC CUỐI nằm lẻ loi ở ngoài vòng lặp của bản cũ ĐÃ BỊ XÓA đi rồi nhé, mọi thứ giờ đã gói gọn bên trong)
    }
}