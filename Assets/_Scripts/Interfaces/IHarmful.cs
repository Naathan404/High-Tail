using UnityEngine;

public interface IHarmful
{
    int Damage { get; }
    float Knockback { get; }
    void DealDamage();
}
