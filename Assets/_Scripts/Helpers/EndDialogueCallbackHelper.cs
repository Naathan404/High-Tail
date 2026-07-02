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
            case (int)Skill.AirGlide:
                _player.UnlockAirGlide();       
                break;     
            case (int)Skill.Dash:
                _player.UnlockDash();
                break;      
            case (int)Skill.AstralLight:
                _player.UnlockAstralLight();  
                break;
            case (int)Skill.Pogo:
                _player.UnlockPogo();
                break;                         
            default:
                break;                    
        }
        UIManager.Instance.SetSkillIcon(skill);
    }

    [System.Serializable]
    public enum Skill
    {
        WallJump, // 0
        WallSlide,       // 1
        AirGlide,           // 2
        Dash,               // 3
        AstralLight,         // 4
        Pogo,              // 5
    }
}
