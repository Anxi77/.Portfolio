using UnityEngine;
using UnityEngine.UI;

public class Monster : MonoBehaviour
{
    public float health { get; set; }
    public float speed;
    public float attackSpeed;
    public float attackRange;
    public float attackDamage;
    public float maxHealth;
    public Rigidbody rb { get; private set; }
    private Player player;
    public Animator anim;

    public Slider healthSlider;

    public float currentHealth { get { return health / maxHealth; } }

    private float attackTimer = 0f;
    private bool canAttack = true;

    private bool isDamaged = false;

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
            anim.SetBool("Moving", true);
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

        if (distanceToPlayer <= attackRange && !isDamaged)
        {
            rb.velocity = Vector3.zero;
            Attack();
        }
        else if (!isDamaged)
        {
            MoveToPlayer();
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        isDamaged = true;
        rb.velocity = Vector3.zero;
        if (health <= 0)
        {
            GameManager.Instance.player.AddExp(100);
            Destroy(gameObject);
            GameManager.Instance.monsterManager.enemies.Remove(this);
        }

        if (anim != null)
        {

            anim.SetBool("Moving", false);
            anim.SetTrigger("Damage");
        }

        healthSlider.value = currentHealth;

        isDamaged = false;
    }

    public void Attack()
    {
        if (canAttack)
        {
            if (anim != null)
            {
                anim.SetBool("Moving", false);
                anim.SetTrigger("Attack");
            }

            player.TakeDamage(attackDamage);

            canAttack = false;
            attackTimer = 0f;
        }
    }
}