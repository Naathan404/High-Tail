using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    [Header("Skills Unlock")]   // =========================================================
    public bool WallJumpUnlocked = false;
    public bool WallSlideUnlocked = false;
    public bool DashUnlocked = false;
    public bool AirGlideUnlocked = false;
    public bool PogoUnlocked = false;

    [Header("Skill Names")]
    [SerializeField] private Text _wallJumpSlideName;
    [SerializeField] private Text _dashName;
    [SerializeField] private Text _airGlideName;
    [SerializeField] private Text _pogoName;

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
}
