using System;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    [Header("Skill Names")]
    [SerializeField] private Text _wallJumpName;
    [SerializeField] private Text _wallJumpDes;
    [SerializeField] private Text _wallSlideName;
    [SerializeField] private Text _wallSlideDes;
    [SerializeField] private Text _dashName;
    [SerializeField] private Text _dashDes;
    [SerializeField] private Text _airGlideName;
    [SerializeField] private Text _airGlideDes;
    [SerializeField] private Text _pogoName;
    [SerializeField] private Text _pogoDes;
    [SerializeField] private Text _glowName;
    [SerializeField] private Text _glowDes;
    [SerializeField] private Text _doubleJumpName;
    [SerializeField] private Text _doubleJumpDes;
    [SerializeField] private Text _astralPulseName;
    [SerializeField] private Text _astralPulseDes;

    public void UnlockWallJump()
    {
        Data.WallJumpUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_wallJumpName, _wallJumpDes);
    }

    public void UnlockWallSlide()
    {
        Data.WallSlideUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_wallSlideName, _wallSlideDes);
    }

    public void UnlockDash()
    {
        Data.DashUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_dashName, _dashDes);
    }
    public void UnlockAirGlide()
    {
        Data.AirGlideUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_airGlideName, _airGlideDes);
    }
    public void UnlockPogo()
    {
        Data.PogoUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_pogoName, _pogoDes);
    }

    public void UnlockGlow()
    {
        Data.GlowUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_glowName, _glowDes);
    }

    public void UnlockDoubleJump()
    {
        Data.DoubleJumpUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_doubleJumpName, _doubleJumpDes);
    }

    public void UnlockAstralLight()
    {
        Data.AstralPulseUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_astralPulseName, _astralPulseDes);
    }
}
