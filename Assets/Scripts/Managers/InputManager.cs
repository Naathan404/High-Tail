using UnityEngine;

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
        Inputs.Respawn.Enable();
    }

    private void OnDisable()
    {
        Inputs.Movement.Disable();
        Inputs.Camera.Disable();
        Inputs.Interaction.Disable();
        Inputs.Respawn.Disable();
    }

    public void DisableControl()
    {
        Inputs.Movement.Disable();
        Inputs.Camera.Disable();
        Inputs.Respawn.Disable();
    }

    public void EnableControl()
    {
        Inputs.Movement.Enable();
        Inputs.Camera.Enable();
        Inputs.Interaction.Enable();
        Inputs.Respawn.Enable();
    }
}
