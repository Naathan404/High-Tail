using UnityEngine;

[CreateAssetMenu(fileName = "Player Stats",menuName = "High Tail/Player Stats")]
public class PlayerData : ScriptableObject
{
    // chỉ số
    [Header("Player Stats")]
    public float maxMoveSpeed;
    public float jumpForce;
    public float dashForce;
    public int maxHP;
    public int maxEnergy;

    // game feel
    [Header("Game Feel Enhancements")]
    public float jumpBufferTime;
    public float coyoteTime;
    public float gravityScale;
    public float fallMultiplier;
    public float acceleration;
    public float decceleration;
    public float boostVelocity;
    public float velocityPower;
    public float dashDuration;

    // năng lượng
    [Header("Enery Consumption & Regeneration")]
    public int energyCostJump;
    public int energyCostDash;
    public int energyRegenPerSecond;
}
