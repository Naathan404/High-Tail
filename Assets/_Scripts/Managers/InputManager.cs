using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    public PlayerControls Inputs;

    private void Awake()
    {
        if(Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        
        Inputs = new PlayerControls();
    }

    private void OnEnable()
    {
        Inputs.Movement.Enable();
        Inputs.Camera.Enable();
        Inputs.Interaction.Enable();
        Inputs.Testing.Enable();
        Inputs.UI.Enable();
    }

    private void OnDisable()
    {
        Inputs.Movement.Disable();
        Inputs.Camera.Disable();
        Inputs.Interaction.Disable();
        Inputs.Testing.Disable();
        Inputs.UI.Disable();
    }

    public void DisableControl()
    {
        Inputs.Movement.Disable();
        Inputs.Camera.Disable();
        //Inputs.Testing.Disable();
    }

    public void EnableControl()
    {
        Inputs.Movement.Enable();
        Inputs.Camera.Enable();
        Inputs.Interaction.Enable();
        Inputs.Testing.Enable();
    }

    public void LoadBindingOverrides(string json)
    {
        if (!string.IsNullOrEmpty(json))
        {
            Inputs.asset.LoadBindingOverridesFromJson(json);
        }
    }

    public string GetBindingOverridesJson()
    {
        return Inputs.asset.SaveBindingOverridesAsJson();
    }
}
