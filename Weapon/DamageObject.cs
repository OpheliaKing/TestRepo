using UnityEngine;

public class DamageObject : MonoBehaviour
{
    private float _damage;

    public float Damage => _damage;

    public void SetDamage(float damage)
    {
        _damage = damage;
    }
}
