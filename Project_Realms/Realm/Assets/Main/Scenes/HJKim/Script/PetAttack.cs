using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;




public class PetAttack : MonoBehaviour
{
    public float damage;//펫의 공격력을 의미함
    public bool canAttack;
    private Coroutine Attack;
    public Animator animator;


    public void Start()
    {
        damage = 25;
        animator = GetComponent<Animator>();
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<Enemy>(out Enemy enemy) && canAttack)
        {
            Attack = StartCoroutine(AttackDelay(enemy));
        }
    }

    public IEnumerator AttackDelay(Enemy enemy)
    {
        canAttack = false;

        if (animator != null)
        {
            animator.SetTrigger("Attack"); // "Attack" 트리거 발동
        }

        enemy.TakeDamage(damage);
        yield return new WaitForSeconds(1);
        canAttack = true;
    }


}
