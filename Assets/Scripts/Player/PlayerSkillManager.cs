using System;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    [Header("Skills Unlock")]   // =========================================================
    public bool WallJumpUnlocked = false;
    public bool WallSlideUnlocked = false;
    public bool DashUnlocked = false;
    public bool AirGlideUnlocked = false;
    public bool PogoUnlocked = false;
    public bool GlowUnlocked = false;
    public bool DoubleJumpUnlocked = false;

    [Header("Skill Names")]
    [SerializeField] private Text _wallJumpSlideName;
    [SerializeField] private Text _dashName;
    [SerializeField] private Text _airGlideName;
    [SerializeField] private Text _pogoName;
    [SerializeField] private Text _glowName;
    [SerializeField] private Text _doubleJumpName;

    public void UnlockWallJumpWallSlide()
    {
        WallJumpUnlocked = WallSlideUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_wallJumpSlideName);
    }
    public void UnlockDash()
    {
        DashUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_dashName);
    }
    public void UnlockAirGlide()
    {
        AirGlideUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_airGlideName);
    }
    public void UnlockPogo()
    {
        PogoUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_pogoName);
    }

    public void UnlockGlow()
    {
        GlowUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_glowName);
    }

    public void UnlockDoubleJump()
    {
        DoubleJumpUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_doubleJumpName);
    }
}
