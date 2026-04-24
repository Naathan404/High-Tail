using UnityEngine;

[CreateAssetMenu(fileName = "Player Stats",menuName = "High Tail/Player Stats")]
public class PlayerData : ScriptableObject
{
    // chỉ số
    [Header("Player Stats")]

    [Header("Skills Unlock")]   // =========================================================
    public bool WallJumpUnlocked = false;
    public bool WallSlideUnlocked = false;
    public bool DashUnlocked = false;
    public bool AirGlideUnlocked = false;
    public bool PogoUnlocked = false;
    public bool GlowUnlocked = false;
    public bool DoubleJumpUnlocked = false;

    [Header("Run")]
    public float maxMoveSpeed = 10;
    public float acceleration = 50;
    public float deceleration = 90;
    public float boostVelocity = 3;
    public float velocityPower = 0.9f;

    [Header("Jump")]
    public float jumpForce = 25;
    public float upperJumpForce = 40;
    public Vector2 jumpWallForce = new Vector2(12, 20);

    [Header("Fall")]
    public float gravityScale = 1;
    public float fallMultiplier = 8;

    [Header("Dash")]
    public float dashForce = 50;
    public float dashDuration = 0.15f;

    [Header("Bounce/Push")]
    public float bounceHorizontalForce = 10;
    public float bounceVerticalForce = 5;

    [Header("Air Glide")]
    public float glideAcceleration = 5;
    public float airGlideGravityScale = 0.3f;
    public float maxGlideSpeed = 4;

    [Header("Wall Slide")]
    public float wallSlideGravityScale = 0.5f;

    [Header("Pogo")]
    public float pogoForce = 20;

    [Header("Vine")]
    public float climbForce = 5f;
    public float swingForce = 60f;
    public float impulseForce = 10f;
    public float kickbackForce = 10f;


    // game feel
    [Header("Others")]
    public float jumpBufferTime = 0.15f;
    public float coyoteTime = 0.1f;
    public float respawnDuration = 0.5f;

    [Range(0, 100)]
    public float maxSpeedX = 200f;
    [Range(0, 100)]
    public float maxSpeedY = 25f; // The player's max speed in any direction, to limit the speed

    // năng lượng
    [Header("Energy Consumption & Regeneration")]
    public int maxHP = 5;
    public int maxEnergy = 100;
    public int energyCostJump = 4;
    public int energyCostDash = 20;
    public int energyRegenPerSecond = 1;
}
