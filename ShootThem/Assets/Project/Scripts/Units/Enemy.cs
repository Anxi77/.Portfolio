using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float Health;
    public float Damage;
    public float Speed;
    public float AttackSpeed;
    public float AttackRange;
    public float AttackDamage;

    public void TakeDamage(float damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Destroy(gameObject);
        }
    }
}