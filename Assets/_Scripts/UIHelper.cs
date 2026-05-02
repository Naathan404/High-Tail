using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class UIHelper
{
    // Chuyển hàm AnimatePanel lúc nãy vào đây và đổi thành public static
    public static void AnimateFade(GameObject obj, bool isActive)
    {
        if (obj == null) return;
        if (obj.activeSelf == isActive) return;

        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();

        LayoutElement layout = obj.GetComponent<LayoutElement>();
        if (layout == null) layout = obj.AddComponent<LayoutElement>();

        // Chốt chặn trạng thái
        if (isActive && obj.activeSelf && cg.alpha >= 0.95f) return; 
        if (!isActive && !obj.activeSelf) return;                    

        obj.transform.DOKill();
        cg.DOKill();

        if (isActive)
        {
            obj.SetActive(true);
            layout.ignoreLayout = false;

            if (cg.alpha == 0) cg.alpha = 0f; 
            if (obj.transform.localScale.x < 0.95f) obj.transform.localScale = Vector3.one * 0.95f;

            cg.DOFade(1f, 0.3f).SetEase(Ease.OutQuad).SetUpdate(true);
            obj.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
        }
        else
        {
            layout.ignoreLayout = true;
            
            cg.DOFade(0f, 0.2f).SetEase(Ease.InQuad).SetUpdate(true);
            obj.transform.DOScale(Vector3.one * 0.95f, 0.2f).SetEase(Ease.InBack).SetUpdate(true)
                .OnComplete(() => obj.SetActive(false));
        }
    }

    public static void AnimateZoom(GameObject obj, bool isActive, float duration = 0.3f)
    {
        if (obj == null) return;
        if (obj.activeSelf == isActive) return;

        // Chốt chặn trạng thái an toàn
        if (isActive && obj.activeSelf && obj.transform.localScale.x >= 0.95f) return;
        if (!isActive && !obj.activeSelf) return;

        // Dừng mọi hiệu ứng cũ
        obj.transform.DOKill();

        if (isActive)
        {
            obj.SetActive(true);
            
            // Nếu đang to sẵn (bị lỗi ngầm) thì ép dẹp về 0 trước khi nảy lên
            if (obj.transform.localScale.x > 0.95f) obj.transform.localScale = Vector3.zero;

            obj.transform.DOScale(Vector3.one, duration).SetEase(Ease.OutBack).SetUpdate(true);
        }
        else
        {
            obj.transform.DOScale(Vector3.zero, duration * 0.8f).SetEase(Ease.InBack).SetUpdate(true)
                .OnComplete(() => obj.SetActive(false));
        }
    }

    public static void PunchScale(GameObject obj, float duration = 0.3f)
    {
        if (obj == null || !obj.activeSelf) return;

        obj.transform.DOKill(true); // Complete tween cũ ngay lập tức để tránh lỗi scale bị kẹt
        
        // Nảy to lên 20% (1.2x) rồi bật về chỗ cũ
        obj.transform.DOPunchScale(Vector3.one * 0.2f, duration, vibrato: 2, elasticity: 0.5f);
    }

    public static void SetTextAnimated(TextMeshProUGUI textComponent, string newText, float duration = 0.3f)
    {
        if (textComponent == null) return;

        // Nếu chữ truyền vào giống hệt chữ cũ thì bỏ qua, không cần chạy hiệu ứng
        if (textComponent.text == newText) return;

        // Dừng mọi hiệu ứng cũ đang chạy trên chữ này để tránh lỗi giật hình
        textComponent.transform.DOKill();

        // Hiệu ứng: Ép dẹp chữ xuống trục Y (tốc độ bằng nửa thời gian)
        textComponent.transform.DOScaleY(0f, duration / 2f).SetEase(Ease.InQuad).OnComplete(() =>
        {
            // Khi chữ đã dẹp lép (không nhìn thấy), tiến hành đổi nội dung
            textComponent.SetText(newText);

            // Bật nảy chữ trở lại kích thước cũ
            textComponent.transform.DOScaleY(1f, duration / 2f).SetEase(Ease.OutBack);
        });
    }

    public static void AnimatePulse(GameObject obj, bool isActive, float maxScale = 1.2f, float duration = 0.5f)
    {
        if (obj == null) return;

        string tweenId = obj.GetInstanceID() + "_cleanpulse";

        if (isActive)
        {
            // Chốt chặn: Nếu đang chạy rồi thì bỏ qua, chống giật hình
            if (DOTween.IsTweening(tweenId)) return;

            // Dọn dẹp trạng thái cũ
            obj.transform.DOKill();

            // Phóng to đều đặn và mượt mà (InOutSine)
            obj.transform.DOScale(Vector3.one * maxScale, duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetId(tweenId); // Gắn ID để dễ dàng dập tắt
        }
        else
        {
            // Dập tắt chính xác loop này
            DOTween.Kill(tweenId);
            obj.transform.DOKill();

            // Trả về kích thước gốc một cách nhanh gọn
            obj.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad);
        }
    }
}