using UnityEngine;

public class Void : MonoBehaviour, IHarmful
{
    public int Damage => throw new System.NotImplementedException();

    public float Knockback => throw new System.NotImplementedException();

    public void DealDamage()
    {
        throw new System.NotImplementedException();
    }
}
