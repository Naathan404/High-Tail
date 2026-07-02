using System;
using UnityEngine;
using UnityEngine.InputSystem;
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
    // [SerializeField] private LocalizedString _glowName;
    // [SerializeField] private LocalizedString _glowDes;
    // [SerializeField] private LocalizedString _doubleJumpName;
    // [SerializeField] private LocalizedString _doubleJumpDes;
    [SerializeField] private LocalizedString _astralPulseName;
    [SerializeField] private LocalizedString _astralPulseDes;

    [Header("Skill Key Bindings")]
    [SerializeField] private InputActionReference _wallJumpActionRef;
    [SerializeField] private InputActionReference _dashActionRef;
    [SerializeField] private InputActionReference _airGlideActionRef;
    [SerializeField] private InputActionReference _pogoActionRef;
    // [SerializeField] private InputActionReference _doubleJumpActionRef;
    [SerializeField] private InputActionReference _astralPulseActionRef;

    public void UnlockWallJump()
    {
        Data.WallJumpUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_wallJumpName, _wallJumpDes, _wallJumpActionRef);
    }

    public void UnlockWallSlide()
    {
        Data.WallSlideUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_wallSlideName, _wallSlideDes, _wallJumpActionRef);
    }

    public void UnlockDash()
    {
        Data.DashUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_dashName, _dashDes, _dashActionRef);
    }
    public void UnlockAirGlide()
    {
        Data.AirGlideUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_airGlideName, _airGlideDes, _airGlideActionRef);
    }
    public void UnlockPogo()
    {
        Data.PogoUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_pogoName, _pogoDes, _pogoActionRef);
    }

    // public void UnlockGlow()
    // {
    //     Data.GlowUnlocked = true;
    //     UIManager.Instance.ShowSkillUnlocked(_glowName, _glowDes, _as);
    // }

    // public void UnlockDoubleJump()
    // {
    //     Data.DoubleJumpUnlocked = true;
    //     UIManager.Instance.ShowSkillUnlocked(_doubleJumpName, _doubleJumpDes);
    // }

    public void UnlockAstralLight()
    {
        Data.AstralPulseUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_astralPulseName, _astralPulseDes, _astralPulseActionRef);
    }
}
