using UnityEngine;

public interface IPushable
{
    Vector2 PushForce { get; }
    float IsRight { get; }
    void PlayPushAnimation();

}
