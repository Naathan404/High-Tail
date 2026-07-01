using UnityEngine;
using UnityEngine.InputSystem;

public static class InputDisplayUtils
{
    public static string GetBindingDisplayString(InputActionReference actionRef, int bindingIndex = 0)
    {
        if (actionRef == null || InputManager.Instance == null || InputManager.Instance.Inputs == null)
            return "?";

        var liveAction = InputManager.Instance.Inputs.asset.FindAction(actionRef.action.id);
        if (liveAction == null || bindingIndex >= liveAction.bindings.Count)
            return "?";

        return InputControlPath.ToHumanReadableString(
            liveAction.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
    }
}