using UnityEngine;

[CreateAssetMenu(fileName = "Player Stats",menuName = "High Tail/Player Stats")]
public class PlayerData : ScriptableObject
{
    // chỉ số
    public float maxMoveSpeed;
    public float jumpForce;
    public float dashForce;
    public int maxHP;
    public int maxEnergy;

    // game feel
    public float jumpBufferTime;
    public float coyoteTime;
    public float gravityScale;
    public float fallMultiplier;
    public float acceleration;
    public float decceleration;
    public float velocityPower;

    // năng lượng
    public int energyCostJump;
    public int energyCostDash;
    public int energyRegenPerSecond;
}
