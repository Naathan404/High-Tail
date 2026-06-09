using System;
using UnityEngine;
using UnityEngine.Localization;

public partial class PlayerController : MonoBehaviour
{
    [Header("Skill Names")]
    [SerializeField] private LocalizedString _wallJumpName;
    [SerializeField] private LocalizedString _wallJumpDes;
    [SerializeField] private LocalizedString _wallSlideName;
    [SerializeField] private LocalizedString _wallSlideDes;
    [SerializeField] private LocalizedString _dashName;
    [SerializeField] private LocalizedString _dashDes;
    [SerializeField] private LocalizedString _airGlideName;
    [SerializeField] private LocalizedString _airGlideDes;
    [SerializeField] private LocalizedString _pogoName;
    [SerializeField] private LocalizedString _pogoDes;
    [SerializeField] private LocalizedString _glowName;
    [SerializeField] private LocalizedString _glowDes;
    [SerializeField] private LocalizedString _doubleJumpName;
    [SerializeField] private LocalizedString _doubleJumpDes;
    [SerializeField] private LocalizedString _astralPulseName;
    [SerializeField] private LocalizedString _astralPulseDes;

    public void UnlockWallJump()
    {
        Data.WallJumpUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_wallJumpName, _wallJumpDes, "SPACE");
    }

    public void UnlockWallSlide()
    {
        Data.WallSlideUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_wallSlideName, _wallSlideDes);
    }

    public void UnlockDash()
    {
        Data.DashUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_dashName, _dashDes, "X");
    }
    public void UnlockAirGlide()
    {
        Data.AirGlideUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_airGlideName, _airGlideDes, "SHIFT");
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
