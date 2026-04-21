using System;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    [Header("Skill Names")]
    [SerializeField] private Text _wallJumpSlideName;
    [SerializeField] private Text _dashName;
    [SerializeField] private Text _airGlideName;
    [SerializeField] private Text _pogoName;
    [SerializeField] private Text _glowName;
    [SerializeField] private Text _doubleJumpName;

    public void UnlockWallJumpWallSlide()
    {
        Data.WallJumpUnlocked = Data.WallSlideUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_wallJumpSlideName);
    }
    public void UnlockDash()
    {
        Data.DashUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_dashName);
    }
    public void UnlockAirGlide()
    {
        Data.AirGlideUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_airGlideName);
    }
    public void UnlockPogo()
    {
        Data.PogoUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_pogoName);
    }

    public void UnlockGlow()
    {
        Data.GlowUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_glowName);
    }

    public void UnlockDoubleJump()
    {
        Data.DoubleJumpUnlocked = true;
        UIManager.Instance.ShowSkillUnlocked(_doubleJumpName);
    }
}
