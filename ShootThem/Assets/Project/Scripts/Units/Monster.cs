using UnityEngine;
using UnityEngine.UI;

public class Monster : MonoBehaviour
{
    public float health { get; set; }
    public float damage;
    public float speed;
    public float attackSpeed;
    public float attackRange;
    public float attackDamage;
    public float maxHealth;
    private Rigidbody rb;
    private Player player;
    public Animator anim;

    public Slider healthSlider;

    private bool isAttacking = false;

    public float currentHealth { get { return health / maxHealth; } }

    private float attackTimer = 0f;
    private bool canAttack = true;

    public void Init(Player player)
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody>();
        this.player = player;
    }

    private void MoveToPlayer()
    {
        rb.velocity = (player.playerPos.position - transform.position).normalized * speed;

        if (anim != null)
        {
            anim.SetTrigger("Move");
            anim.SetFloat("Speed", rb.velocity.magnitude);
        }
    }

    private void LookAtPlayer()
    {
        Vector3 direction = player.playerPos.position - transform.position;
        direction.y = 0;
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5f);
    }

    private void Update()
    {
        LookAtPlayer();

        if (!canAttack)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= 1f / attackSpeed)
            {
                canAttack = true;
            }
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.playerPos.position);

        if (distanceToPlayer <= attackRange)
        {
            rb.velocity = Vector3.zero;
            Attack();
        }
        else
        {
            isAttacking = false;
            MoveToPlayer();
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(gameObject);
            GameManager.Instance.monsterManager.enemies.Remove(this);
        }

        healthSlider.value = currentHealth;
    }

    public void Attack()
    {
        print("Attack");
        if (canAttack)
        {
            isAttacking = true;

            if (anim != null)
            {
                anim.SetTrigger("Attack");
            }
            //player.TakeDamage(attackDamage);

            canAttack = false;
            attackTimer = 0f;
        }
    }
}