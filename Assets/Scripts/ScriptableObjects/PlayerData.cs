using UnityEngine;

[CreateAssetMenu(fileName = "Player Stats",menuName = "High Tail/Player Stats")]
public class PlayerData : ScriptableObject
{
    // chỉ số
    [Header("Player Stats")]
    public float maxMoveSpeed = 10;
    public float jumpForce = 25;
    public float upperJumpForce = 40;
    public float jumpWallForce = 12;
    public float dashForce = 30;
    public float wallSlideGravityScale = 0.5f;
    public float airGlideGravityScale = 0.3f;
    public float maxGlideSpeed = 4;
    public float pogoForce = 20;
    public int maxHP = 5;
    public int maxEnergy = 100;

    // game feel
    [Header("Game Feel Enhancements")]
    public float jumpBufferTime = 0.15f;
    public float coyoteTime = 0.1f;
    public float gravityScale = 1;
    public float fallMultiplier = 8;
    public float acceleration = 50;
    public float decceleration = 90;
    public float boostVelocity = 3;
    public float velocityPower = 0.9f;
    public float dashDuration = 0.15f;
    public float bounceHorizontalForce = 10;
    public float bounceVerticalForce = 5;
    public float glideAccelaration = 5;

    // năng lượng
    [Header("Enery Consumption & Regeneration")]
    public int energyCostJump = 4;
    public int energyCostDash = 20;
    public int energyRegenPerSecond = 1;
}
