using System;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    public event Action OnStepIn;
    public event Action OnStepOut;

    public void ExecuteTrigger()
    {
        OnStepIn?.Invoke();
    }

    public void ExitTrigger()
    {
        OnStepOut?.Invoke();
    }
}
