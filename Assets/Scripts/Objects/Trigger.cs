using System;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    public event Action OnStepIn;

    public void ExecuteTrigger()
    {
        OnStepIn?.Invoke();
    }
}
