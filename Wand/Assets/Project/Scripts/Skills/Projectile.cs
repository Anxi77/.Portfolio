using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifeTime = 5f;

    public ParticleSystem hitEffect;

    public void Init(float damage)
    {
        this.damage = damage;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<Monster>(out Monster monster))
        {
            monster.TakeDamage(damage);
            monster.rb.AddForce(transform.forward * 100f, ForceMode.Impulse);
            hitEffect = Instantiate(hitEffect, transform.position, Quaternion.identity);
            hitEffect.Play();
            Destroy(hitEffect.gameObject, 1f);
            Destroy(gameObject);
        }
    }
}