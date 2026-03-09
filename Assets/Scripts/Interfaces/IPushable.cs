using UnityEngine;

public interface IPushable
{
    float PushForce { get; }
    float IsRight { get; }
    void PlayPushAnimation();

}
