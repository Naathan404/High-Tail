using UnityEngine;

public class EndDialogueCallbackHelper : MonoBehaviour
{
    private PlayerController _player;
    private void Start()
    {
        _player = FindAnyObjectByType<PlayerController>();
    }
    public void UnlockSkill(int skill)
    {
        switch(skill)
        {
            case (int)Skill.WallJumpWallSlide:
                _player.UnlockWallJumpWallSlide();
                break;
            case (int)Skill.Dash:
                _player.UnlockDash();
                break;      
            case (int)Skill.AirGlide:
                _player.UnlockAirGlide();
                break;            
            case (int)Skill.Glow:
                _player.UnlockGlow();
                break;     
            case (int)Skill.Pogo:
                _player.UnlockPogo();
                break;               
            case (int)Skill.DoubleJump:
                _player.UnlockDoubleJump();
                break;                  
            default:
                break;                    
        }
    }

    [System.Serializable]
    public enum Skill
    {
        WallJumpWallSlide,
        Dash,
        AirGlide, 
        Glow,
        Pogo,
        DoubleJump
    }
}
