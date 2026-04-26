using System;
using UnityEngine;

public class Spike : MonoBehaviour, IHarmful
{
    [SerializeField] private int _damage;
    [SerializeField] private float _knockback;
    public int Damage => _damage;
    public float Knockback => _knockback;

    public void DealDamage()
    {

    }
}
