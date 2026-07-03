using UnityEngine;

public class Void : MonoBehaviour, IHarmful
{
    public int Damage => 1;

    public float Knockback => 1f;

    public void DealDamage()
    {
        //throw new System.NotImplementedException();
    }
}
