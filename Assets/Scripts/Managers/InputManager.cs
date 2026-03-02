using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    public PlayerControls Inputs;

    private void Awake()
    {
        if(Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        Inputs = new PlayerControls();
    }

    private void OnEnable()
    {
        Inputs.Movement.Enable();
        Inputs.Camera.Enable();
        Inputs.Interaction.Enable();
    }

    private void OnDisable()
    {
        Inputs.Movement.Disable();
        Inputs.Camera.Disable();
        Inputs.Interaction.Disable();        
    }
}
