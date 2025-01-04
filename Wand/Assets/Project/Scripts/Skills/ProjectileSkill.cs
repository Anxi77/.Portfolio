using UnityEngine;

public class ProjectileSkill : Skill
{
    public Projectile projectilePrefab;
    public ParticleSystem muzzleFlash;
    public Transform spawnPoint;
    public float fireRate = 1f;
    private float lastShotTime = 0f;
    protected override void OnSkillActivate()
    {
        ShootProjectile();
    }

    public void ShootProjectile()
    {
        print("ShootProjectile");
        if (Time.time - lastShotTime < 1f / fireRate)
        {
            return;
        }
        lastShotTime = Time.time;
        muzzleFlash.Play();
        Projectile projectile = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);
        projectile.Init(GameManager.Instance.player.Damage);
        player.UseMana(manaCost);
        controller.SendHapticImpulse(0.5f, 0.1f);
    }
}