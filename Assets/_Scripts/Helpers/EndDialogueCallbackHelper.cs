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
            case (int)Skill.WallJump:
                _player.UnlockWallJump();
                break;
            case(int)Skill.WallSlide:
                _player.UnlockWallSlide();
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
            case (int)Skill.AstralLight:
                _player.UnlockAstralLight();  
                break;
            default:
                break;                    
        }
    }

    [System.Serializable]
    public enum Skill
    {
        WallJump, // 0
        Dash,               // 1
        AirGlide,           // 2
        Glow,              // 3 
        Pogo,              // 4
        DoubleJump,          // 5
        AstralLight,         // 6
        WallSlide       // 7
    }
}
